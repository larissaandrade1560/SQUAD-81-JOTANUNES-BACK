using System.Security.Cryptography;
using System.Text;

namespace JotaNunesForms.Application.Convites;

public static class ConviteToken
{
    public static (string RawToken, string TokenHash) CreateRawAndHash()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var raw = Base64UrlEncode(bytes);
        return (raw, HashRaw(raw));
    }

    public static string HashRaw(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
