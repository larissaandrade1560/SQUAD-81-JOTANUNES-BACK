using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JotaNunesForms.Infrastructure.Email;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;
    private readonly IHostEnvironment _environment;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public Task SendAsync(
        string to,
        string subject,
        string textBody,
        string? htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (_environment.IsDevelopment())
        {
            _logger.LogInformation(
                "E-mail (dev): To={To}, Subject={Subject}, TextBody={TextBody}",
                to,
                subject,
                textBody);
        }
        else
        {
            _logger.LogInformation(
                "E-mail simulado: To={To}, Subject={Subject} (corpo omitido)",
                to,
                subject);
        }

        return Task.CompletedTask;
    }
}
