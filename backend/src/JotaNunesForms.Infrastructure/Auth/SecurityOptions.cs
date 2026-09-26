namespace JotaNunesForms.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 480;

    public bool IsSecure => !string.IsNullOrEmpty(SigningKey)
        && SigningKey.Length >= 32
        && !SecurityDefaults.IsWeak(SigningKey)
        && !string.IsNullOrWhiteSpace(Issuer)
        && !string.IsNullOrWhiteSpace(Audience)
        && ExpirationMinutes is > 0 and <= 1440;
}

public sealed class CorsOptions
{
    public const string SectionName = "Cors";
    public string[] Origins { get; set; } = [];

    public bool IsProductionSafe => Origins.Length > 0 && Origins.All(origin =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && string.IsNullOrEmpty(uri.UserInfo)
        && !uri.Host.Contains('*')
        && uri.AbsolutePath == "/"
        && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment)
        && !uri.IsLoopback);
}

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";
    public bool Enabled { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string NomeExibicao { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool HasSafeCredentials => !Enabled
        || (!string.IsNullOrEmpty(Password) && Password.Length >= 16 && !SecurityDefaults.IsWeak(Password)
            && !string.IsNullOrWhiteSpace(Documento)
            && !string.IsNullOrWhiteSpace(NomeExibicao));
}

internal static class SecurityDefaults
{
    private static readonly string[] KnownValues =
    [
        "dev-local-jwt-signing-key-change-me-32chars",
        "senha123",
        "password",
        "changeme",
        "change-me",
        "admin123",
        "secret",
        "qwerty",
        "abcdefghijklmnopqrstuvwxyz",
        "abcdefghijklmnop",
        "1234567890",
    ];

    public static bool IsWeak(string value) => KnownValues.Any(known =>
            value.Contains(known, StringComparison.OrdinalIgnoreCase))
        || value.Distinct().Count() < 8;
}
