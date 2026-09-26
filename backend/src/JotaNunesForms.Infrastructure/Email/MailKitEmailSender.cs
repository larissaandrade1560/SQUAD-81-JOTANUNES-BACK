using JotaNunesForms.Domain.Ports;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JotaNunesForms.Infrastructure.Email;

public sealed class MailKitEmailSender : IEmailSender
{
    private static readonly TimeSpan SmtpOperationTimeout = TimeSpan.FromSeconds(90);

    private readonly EmailOptions _options;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IOptions<EmailOptions> options, ILogger<MailKitEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string textBody,
        string? htmlBody,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_options.From!));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            TextBody = textBody,
            HtmlBody = htmlBody,
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient
        {
            Timeout = (int)SmtpOperationTimeout.TotalMilliseconds,
        };
        using var timeoutCts = new CancellationTokenSource(SmtpOperationTimeout);
        var smtpToken = timeoutCts.Token;

        var socketOptions = ResolveSocketOptions(_options.Port, _options.UseStartTls);
        try
        {
            await client.ConnectAsync(
                _options.Host!,
                _options.Port,
                socketOptions,
                smtpToken);
            await client.AuthenticateAsync(_options.User!, _options.Password!, smtpToken);
            await client.SendAsync(message, smtpToken);
            await client.DisconnectAsync(true, smtpToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail para {To}", to);
            throw;
        }
    }

    private static SecureSocketOptions ResolveSocketOptions(int port, bool useStartTls) =>
        port switch
        {
            465 => SecureSocketOptions.SslOnConnect,
            587 => SecureSocketOptions.StartTls,
            _ => useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto,
        };
}
