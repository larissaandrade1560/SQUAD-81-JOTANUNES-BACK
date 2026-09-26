namespace JotaNunesForms.Api.Tests.Infrastructure;

[CollectionDefinition("Security API PostgreSQL")]
public sealed class SecurityApiCollection : ICollectionFixture<SecurityApiFactory>
{
    public const string Name = "Security API PostgreSQL";
}
