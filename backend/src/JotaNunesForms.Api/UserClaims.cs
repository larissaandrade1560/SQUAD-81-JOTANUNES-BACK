using System.Security.Claims;
using JotaNunesForms.Application.Auth;
using Microsoft.AspNetCore.Http;

namespace JotaNunesForms.Api;

internal static class UserClaims
{
    public static AccessScope GetAccessScope(HttpContext context)
    {
        if (context.Items.TryGetValue("JotaNunesForms.CurrentIdentity", out var value)
            && value is CurrentIdentity identity)
        {
            return AccessScope.From(identity);
        }

        throw new InvalidIdentityException("A identidade corrente não foi resolvida.");
    }

    public static string? GetPerfil(ClaimsPrincipal user) =>
        user.FindFirstValue("perfil");

    public static Guid? GetEmpresaId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue("empresa_id");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    public static bool IsTerceirizado(ClaimsPrincipal user) =>
        GetPerfil(user) == "Terceirizado";

    public static bool IsAdministrador(ClaimsPrincipal user) =>
        GetPerfil(user) == "Administrador";

    public static bool IsEquipeInterna(ClaimsPrincipal user)
    {
        var perfil = GetPerfil(user);
        return perfil is "Administrador" or "Analista";
    }
}
