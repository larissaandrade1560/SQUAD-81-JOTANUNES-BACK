using System.Security.Cryptography;

namespace JotaNunesForms.Application.Documentos;

public static class FileHash
{
    public static string Sha256Hex(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        var hash = SHA256.HashData(stream);
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
