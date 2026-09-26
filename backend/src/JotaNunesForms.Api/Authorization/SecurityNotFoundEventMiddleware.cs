using JotaNunesForms.Application.Auth;
using Microsoft.AspNetCore.Routing;

namespace JotaNunesForms.Api.Authorization;

public sealed class SecurityNotFoundEventMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ISecurityEventSink securityEvents)
    {
        await next(context);
        if (context.Response.StatusCode != StatusCodes.Status404NotFound
            || context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText
            ?? "/unclassified";
        var userId = Guid.TryParse(context.User.FindFirst("usuario_id")?.Value, out var parsedId)
            ? parsedId
            : (Guid?)null;

        await securityEvents.PublishAsync(new SecurityEvent(
            "tenant_not_found",
            "resource_missing_or_out_of_scope",
            context.TraceIdentifier,
            userId,
            context.Request.Method,
            route,
            StatusCodes.Status404NotFound), context.RequestAborted);
    }
}
