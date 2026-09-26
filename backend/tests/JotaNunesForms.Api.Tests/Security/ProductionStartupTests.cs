using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Security;

public sealed class ProductionStartupTests
{
    private const string ValidKey = "nB4rS8xD2hJ6kL9mP3qV7wA1cE5fT0yG";

    [Theory]
    [InlineData("", "https://app.example.test")]
    [InlineData("short", "https://app.example.test")]
    [InlineData("dev-local-jwt-signing-key-change-me-32chars", "https://app.example.test")]
    [InlineData("nB4rS8xD2hJ6kL9mP3qV7wA1cE5fT0yG", "https://*.pages.dev")]
    [InlineData("nB4rS8xD2hJ6kL9mP3qV7wA1cE5fT0yG", "http://localhost:5173")]
    public void ProductionStartupRejectsUnsafeJwtOrCorsWithoutPrintingTheSecret(string key, string origins)
    {
        using var factory = new ProductionApiFactory(key, origins);

        var exception = Assert.ThrowsAny<Exception>(() => factory.CreateClient());

        if (!string.IsNullOrEmpty(key))
        {
            Assert.DoesNotContain(key, exception.ToString(), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ProductionStartupAcceptsValidTypedSecurityConfiguration()
    {
        using var factory = new ProductionApiFactory(ValidKey, "https://app.example.test");

        using var client = factory.CreateClient();

        Assert.True((await client.GetAsync("/")).IsSuccessStatusCode);
    }
}
