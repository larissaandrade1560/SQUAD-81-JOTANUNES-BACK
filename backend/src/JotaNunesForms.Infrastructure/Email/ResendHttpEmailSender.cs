using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JotaNunesForms.Infrastructure.Email;

/// <summary>
/// Resend delivery over HTTPS (same API key as SMTP). No Resend SDK.
/// Prefer on hosts where outbound SMTP is slow or blocked.
/// </summary>
public sealed class ResendHttpEmailSender : IEmailSender
{
    public const string HttpClientName = "resend-http";

    private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(60);

    private readonly EmailOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ResendHttpEmailSender> _logger;

    public ResendHttpEmailSender(
        IOptions<EmailOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<ResendHttpEmailSender> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string textBody,
        string? htmlBody,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(SendTimeout);

        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Password);

        var payload = new ResendSendEmailRequest
        {
            From = _options.From!,
            To = [to],
            Subject = subject,
            Text = textBody,
            Html = htmlBody,
        };
        request.Content = JsonContent.Create(payload);

        try
        {
            using var response = await client.SendAsync(request, timeoutCts.Token);
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            _logger.LogError(
                "Resend HTTP API retornou {StatusCode} ao enviar e-mail para {To}. Corpo: {Body}",
                (int)response.StatusCode,
                to,
                TruncateForLog(body));
            throw new InvalidOperationException($"Resend HTTP API falhou com status {(int)response.StatusCode}.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail para {To} via Resend HTTP API", to);
            throw;
        }
    }

    private static string TruncateForLog(string value) =>
        value.Length <= 500 ? value : value[..500] + "…";

    private sealed class ResendSendEmailRequest
    {
        [JsonPropertyName("from")]
        public required string From { get; init; }

        [JsonPropertyName("to")]
        public required IReadOnlyList<string> To { get; init; }

        [JsonPropertyName("subject")]
        public required string Subject { get; init; }

        [JsonPropertyName("text")]
        public required string Text { get; init; }

        [JsonPropertyName("html")]
        public string? Html { get; init; }
    }
}
