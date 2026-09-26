using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Api.Authorization;

/// <summary>Loads current authorization state once per request; JWT role/company claims are never authoritative.</summary>
public sealed class CurrentIdentityResolver
{
    private readonly IUsuarioRepository _usuarios;
    private Task<CurrentIdentity?>? _current;

    public CurrentIdentityResolver(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public Task<CurrentIdentity?> ResolveAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (_current is not null)
        {
            return _current;
        }

        var userIds = principal.FindAll("usuario_id").Select(claim => claim.Value).ToArray();
        if (userIds.Length != 1 || !Guid.TryParse(userIds[0], out var userId) || userId == Guid.Empty)
        {
            return Task.FromResult<CurrentIdentity?>(null);
        }

        _current = ResolveCoreAsync(principal, userId, cancellationToken);
        return _current;
    }

    private async Task<CurrentIdentity?> ResolveCoreAsync(
        ClaimsPrincipal principal,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var state = await _usuarios.GetSecurityStateByIdAsync(userId, cancellationToken);
        if (state is null)
        {
            return null;
        }

        var usuario = state.Usuario;
        var identity = new CurrentIdentity(
            usuario.Id,
            usuario.Perfil,
            usuario.Ativo,
            usuario.EmpresaId,
            state.EmpresaTipo,
            state.EmpresaAtiva);

        return ClaimsMatchCurrentState(principal, identity, usuario.PerfilRotulo) ? identity : null;
    }

    private static bool ClaimsMatchCurrentState(
        ClaimsPrincipal principal,
        CurrentIdentity identity,
        string profileLabel)
    {
        if (GetSingleClaim(principal, "perfil") != identity.Profile.ToString()
            || GetSingleClaim(principal, "perfil_rotulo") != profileLabel)
        {
            return false;
        }

        var companyIdClaim = GetSingleOptionalClaim(principal, "empresa_id", out var companyIdClaimValid);
        var companyTypeClaim = GetSingleOptionalClaim(principal, "tipo_empresa", out var companyTypeClaimValid);
        if (!companyIdClaimValid || !companyTypeClaimValid)
        {
            return false;
        }

        if (identity.Profile is PerfilUsuario.Administrador or PerfilUsuario.Analista)
        {
            return companyIdClaim is null && companyTypeClaim is null;
        }

        return identity.Profile == PerfilUsuario.Terceirizado
            && identity.CompanyId is Guid companyId
            && Guid.TryParse(companyIdClaim, out var tokenCompanyId)
            && tokenCompanyId == companyId
            && identity.CompanyType is not null
            && companyTypeClaim == identity.CompanyType.Value.ToString();
    }

    private static string? GetSingleClaim(ClaimsPrincipal principal, string claimType)
    {
        var claims = principal.FindAll(claimType).ToArray();
        return claims.Length == 1 ? claims[0].Value : null;
    }

    private static string? GetSingleOptionalClaim(
        ClaimsPrincipal principal,
        string claimType,
        out bool valid)
    {
        var claims = principal.FindAll(claimType).ToArray();
        valid = claims.Length <= 1;
        return claims.Length == 1 ? claims[0].Value : null;
    }
}
