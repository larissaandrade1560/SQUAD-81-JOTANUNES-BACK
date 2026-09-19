using Amazon.S3;
using Amazon.S3.Model;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JotaNunesForms.Infrastructure.Storage;

public sealed class R2ObjectStorage : IObjectStorage, IDisposable
{
    private readonly R2StorageOptions _options;
    private readonly ILogger<R2ObjectStorage> _logger;
    private readonly Lazy<IAmazonS3?> _client;

    public R2ObjectStorage(IOptions<R2StorageOptions> options, ILogger<R2ObjectStorage> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new Lazy<IAmazonS3?>(CreateClient);
    }

    public bool IsConfigured => _options.IsConfigured;

    public async Task UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var client = RequireClient();
        await client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
            },
            cancellationToken);
    }

    public Task<string> GetDownloadUrlAsync(
        string key,
        TimeSpan validFor,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var client = RequireClient();
        var url = client.GetPreSignedURL(
            new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(validFor),
            });
        return Task.FromResult(url);
    }

    private IAmazonS3 RequireClient()
    {
        if (_client.Value is null)
        {
            throw new InvalidOperationException(
                "Armazenamento R2 não configurado. Defina R2__AccountId, R2__AccessKeyId, R2__SecretAccessKey e R2__BucketName.");
        }

        return _client.Value;
    }

    private IAmazonS3? CreateClient()
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning("R2 storage is not configured; document uploads will fail.");
            return null;
        }

        var serviceUrl = string.IsNullOrWhiteSpace(_options.ServiceUrl)
            ? $"https://{_options.AccountId}.r2.cloudflarestorage.com"
            : _options.ServiceUrl;

        var config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            AuthenticationRegion = "auto",
        };

        return new AmazonS3Client(_options.AccessKeyId, _options.SecretAccessKey, config);
    }

    public void Dispose()
    {
        if (_client.IsValueCreated)
        {
            _client.Value?.Dispose();
        }
    }
}
