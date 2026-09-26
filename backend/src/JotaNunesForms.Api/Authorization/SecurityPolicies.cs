using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace JotaNunesForms.Api.Authorization;

public enum AccessRule
{
    ActiveIdentity,
    Administrator,
    Internal,
    TerceirizadoAtivo,
    TerceirizadoMaoDeObra,
    InternalOrOwn,
    InternalOrOwnMaoDeObra,
    InternalOrAny,
    AdministratorOrOwn,
}

public sealed record AccessRequirement(AccessRule Rule) : IAuthorizationRequirement;

public sealed class SecurityPolicyHandler(CurrentIdentityResolver identities, IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<AccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccessRequirement requirement)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var identity = await identities.ResolveAsync(
            context.User,
            httpContext?.RequestAborted ?? CancellationToken.None);

        if (identity is null || !identity.IsActive)
        {
            if (httpContextAccessor.HttpContext is { } invalidContext)
            {
                invalidContext.Items[SecurityAuthorizationResultHandler.DenialReasonItemKey] = "identity_invalid";
            }

            context.Fail();
            return;
        }

        var internalUser = identity.Profile is PerfilUsuario.Administrador or PerfilUsuario.Analista;
        var thirdParty = identity.Profile == PerfilUsuario.Terceirizado;
        var ownCompany = thirdParty && identity.CompanyId is not null && identity.CompanyActive == true;
        var ownMoCompany = ownCompany && identity.CompanyType == TipoEmpresa.MaoDeObra;

        var allowed = requirement.Rule switch
        {
            AccessRule.ActiveIdentity => true,
            AccessRule.Administrator => identity.Profile == PerfilUsuario.Administrador,
            AccessRule.Internal => internalUser,
            AccessRule.TerceirizadoAtivo => ownCompany,
            AccessRule.TerceirizadoMaoDeObra => ownMoCompany,
            AccessRule.InternalOrOwn => internalUser || ownCompany,
            AccessRule.InternalOrOwnMaoDeObra => internalUser || ownMoCompany,
            AccessRule.InternalOrAny => internalUser || ownCompany,
            AccessRule.AdministratorOrOwn => identity.Profile == PerfilUsuario.Administrador || ownCompany,
            _ => false,
        };

        if (allowed)
        {
            context.Succeed(requirement);
        }
        else
        {
            if (httpContextAccessor.HttpContext is { } deniedContext)
            {
                deniedContext.Items[SecurityAuthorizationResultHandler.DenialReasonItemKey] = "capability_denied";
            }

            context.Fail();
        }
    }
}
