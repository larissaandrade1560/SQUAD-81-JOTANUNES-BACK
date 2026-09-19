namespace JotaNunesForms.Infrastructure;

public static class ConnectionStringNormalizer
{
    public static string? Normalize(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.Trim('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        return
            $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
