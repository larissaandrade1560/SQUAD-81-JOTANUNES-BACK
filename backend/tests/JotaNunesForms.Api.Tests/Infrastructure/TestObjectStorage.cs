using System.Collections.Concurrent;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Api.Tests.Infrastructure;

public sealed class TestObjectStorage : IObjectStorage
{
    private readonly ConcurrentDictionary<string, byte[]> _objects = new(StringComparer.Ordinal);
    private int _uploadCount;
    private int _downloadCount;

    public bool IsConfigured => true;

    public int UploadCount => Volatile.Read(ref _uploadCount);

    public int DownloadCount => Volatile.Read(ref _downloadCount);

    public async Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        _objects[key] = buffer.ToArray();
        Interlocked.Increment(ref _uploadCount);
    }

    public Task<string> GetDownloadUrlAsync(
        string key,
        TimeSpan validFor,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _downloadCount);
        return Task.FromResult($"https://storage.security.test/{Uri.EscapeDataString(key)}");
    }

    public bool Contains(string key) => _objects.ContainsKey(key);

    public void ResetCounters()
    {
        Interlocked.Exchange(ref _uploadCount, 0);
        Interlocked.Exchange(ref _downloadCount, 0);
    }
}
