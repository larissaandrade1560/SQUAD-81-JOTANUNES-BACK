using Microsoft.AspNetCore.Authentication;
using JotaNunesForms.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Routing;

namespace JotaNunesForms.Api.Authorization;

public sealed class SecurityAuthorizationResultHandler(
    ISecurityEventSink securityEvents) : IAuthorizationMiddlewareResultHandler
{
    internal const string DenialReasonItemKey = "JotaNunesForms.SecurityDenialReason";

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Succeeded)
        {
            var reason = context.Items.TryGetValue(DenialReasonItemKey, out var value) && value is string code
                ? code
                : "missing_or_invalid_token";
            var isInactiveIdentity = reason == "identity_invalid";
            var challenged = authorizeResult.Challenged || isInactiveIdentity;
            var statusCode = challenged ? StatusCodes.Status401Unauthorized : StatusCodes.Status403Forbidden;
            var eventId = challenged ? "authorization_challenge" : "authorization_forbid";
            var userId = Guid.TryParse(context.User.FindFirst("usuario_id")?.Value, out var parsedId)
                ? parsedId
                : (Guid?)null;
            var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText
                ?? "/unclassified";

            await securityEvents.PublishAsync(new SecurityEvent(
                eventId,
                reason,
                context.TraceIdentifier,
                userId,
                context.Request.Method,
                route,
                statusCode), context.RequestAborted);

            if (challenged)
            {
                await context.ChallengeAsync();
                return;
            }

            await context.ForbidAsync();
            return;
        }

        await next(context);
    }
}
