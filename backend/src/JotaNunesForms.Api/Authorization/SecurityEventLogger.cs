using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Api.Authorization;

public sealed class SecurityEventLogger(ILogger<SecurityEventLogger> logger) : ISecurityEventSink
{
    public ValueTask PublishAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Security event {EventId}: {ReasonCode}; TraceId {TraceId}; UserId {UserId}; {Method} {Route}; status {StatusCode}",
            securityEvent.EventId,
            securityEvent.ReasonCode,
            securityEvent.TraceId,
            securityEvent.UserId,
            securityEvent.Method,
            securityEvent.Route,
            securityEvent.StatusCode);
        return ValueTask.CompletedTask;
    }
}
