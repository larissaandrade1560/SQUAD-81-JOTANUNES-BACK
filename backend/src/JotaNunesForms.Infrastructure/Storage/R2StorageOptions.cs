namespace JotaNunesForms.Infrastructure.Storage;

public sealed class R2StorageOptions
{
    public string? AccountId { get; set; }

    public string? AccessKeyId { get; set; }

    public string? SecretAccessKey { get; set; }

    public string? BucketName { get; set; }

    public string? ServiceUrl { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(AccountId)
        && !string.IsNullOrWhiteSpace(AccessKeyId)
        && !string.IsNullOrWhiteSpace(SecretAccessKey)
        && !string.IsNullOrWhiteSpace(BucketName);
}
