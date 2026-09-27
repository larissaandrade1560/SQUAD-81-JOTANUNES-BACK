using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JotaNunesForms.Application.Mobilizacoes;

public static class IdempotencyPayload
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static string ComputeHash<T>(T payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var canonical = JsonSerializer.Serialize(payload, SerializerOptions);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool MatchesExistingHash(string existingHash, string computedHash) =>
        string.Equals(existingHash, computedHash, StringComparison.OrdinalIgnoreCase);
}
