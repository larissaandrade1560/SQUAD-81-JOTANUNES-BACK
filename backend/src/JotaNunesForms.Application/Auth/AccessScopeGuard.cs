using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Auth;

public sealed class AccessScopeGuard(ISecurityEventSink securityEvents)
{
    public async Task<bool> AllowsCompanyAsync(
        AccessScope scope,
        Guid resourceCompanyId,
        SecurityRequestContext request,
        CancellationToken cancellationToken = default)
    {
        if (scope is not AccessScope.Company company || company.CompanyId == resourceCompanyId)
        {
            return true;
        }

        await securityEvents.PublishAsync(
            new SecurityEvent(
                "tenant_access_denied",
                "tenant_mismatch",
                request.TraceId,
                company.UserId,
                request.Method,
                request.Route,
                StatusCode: 404),
            cancellationToken);
        return false;
    }
}
