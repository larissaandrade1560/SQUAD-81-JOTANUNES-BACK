using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Infrastructure.Email;

public sealed class UnavailableEmailSender : IEmailSender
{
    public Task SendAsync(
        string to,
        string subject,
        string textBody,
        string? htmlBody,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Serviço de e-mail não configurado.");
}
