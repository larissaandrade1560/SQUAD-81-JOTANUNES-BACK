namespace JotaNunesForms.Infrastructure.Email;

public sealed class EmailOptions
{
    public string? Host { get; set; }

    public int Port { get; set; } = 587;

    public string? User { get; set; }

    public string? Password { get; set; }

    public string? From { get; set; }

    public bool UseStartTls { get; set; } = true;

    /// <summary>
    /// When true, send via Resend HTTPS API (<c>Email__Password</c> = API key) instead of SMTP.
    /// </summary>
    public bool UseHttpApi { get; set; }

    public bool IsHttpApiConfigured =>
        UseHttpApi
        && !string.IsNullOrWhiteSpace(Password)
        && !string.IsNullOrWhiteSpace(From);

    public bool IsSmtpConfigured =>
        !UseHttpApi
        && !string.IsNullOrWhiteSpace(Host)
        && Port > 0
        && !string.IsNullOrWhiteSpace(User)
        && !string.IsNullOrWhiteSpace(Password)
        && !string.IsNullOrWhiteSpace(From);

    public bool IsConfigured => IsHttpApiConfigured || IsSmtpConfigured;
}
