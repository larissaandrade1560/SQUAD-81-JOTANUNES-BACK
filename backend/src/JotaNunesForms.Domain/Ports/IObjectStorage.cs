namespace JotaNunesForms.Domain.Ports;

public interface IObjectStorage
{
    Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<string> GetDownloadUrlAsync(
        string key,
        TimeSpan validFor,
        CancellationToken cancellationToken = default);

    bool IsConfigured { get; }
}
