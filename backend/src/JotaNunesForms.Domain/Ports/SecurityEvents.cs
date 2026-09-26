namespace JotaNunesForms.Domain.Ports;

public sealed record SecurityEvent(
    string EventId,
    string ReasonCode,
    string TraceId,
    Guid? UserId = null,
    string? Method = null,
    string? Route = null,
    int? StatusCode = null);

public interface ISecurityEventSink
{
    ValueTask PublishAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default);
}
