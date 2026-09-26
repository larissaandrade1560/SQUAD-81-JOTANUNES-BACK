using JotaNunesForms.Infrastructure.Auth;

namespace JotaNunesForms.Infrastructure.Tests;

public sealed class SecurityOptionsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("short")]
    [InlineData("dev-local-jwt-signing-key-change-me-32chars")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    public void JwtOptionsRejectsMissingShortKnownAndLowDiversityKeys(string key)
    {
        var options = new JwtOptions
        {
            SigningKey = key,
            Issuer = "issuer",
            Audience = "audience",
        };

        Assert.False(options.IsSecure);
    }

    [Fact]
    public void JwtOptionsAcceptsAUniqueKeyOfAtLeastThirtyTwoCharacters()
    {
        var options = new JwtOptions
        {
            SigningKey = "sR8mV4qN2xJ7pL9cA6dF1hK5wB3eT0yG",
            Issuer = "JotaNunesForms",
            Audience = "JotaNunesForms.Web",
            ExpirationMinutes = 480,
        };

        Assert.True(options.IsSecure);
    }

    [Theory]
    [InlineData("http://app.example.test")]
    [InlineData("https://localhost:5173")]
    [InlineData("https://*.pages.dev")]
    [InlineData("https://app.example.test/path")]
    public void CorsOptionsRejectsInsecureOrNonOriginEntries(string origin)
    {
        Assert.False(new CorsOptions { Origins = [origin] }.IsProductionSafe);
    }

    [Fact]
    public void CorsOptionsAcceptsAnExactHttpsOrigin()
    {
        Assert.True(new CorsOptions { Origins = ["https://app.example.test"] }.IsProductionSafe);
    }

    [Theory]
    [InlineData("")]
    [InlineData("senha123")] 
    [InlineData("abcdefghijklmnop")]
    public void BootstrapOptionsRejectsMissingShortKnownOrPredictablePassword(string password)
    {
        var options = new BootstrapAdminOptions
        {
            Enabled = true,
            Documento = "12345678900",
            NomeExibicao = "Administrator",
            Password = password,
        };

        Assert.False(options.HasSafeCredentials);
    }
}
