using System.Net;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Security;

public sealed class ProductionExposureTests
{
    [Fact]
    public async Task SwaggerIsHiddenAndCorsAllowsOnlyTheConfiguredOrigin()
    {
        using var factory = new ProductionApiFactory(
            "nB4rS8xD2hJ6kL9mP3qV7wA1cE5fT0yG",
            "https://app.example.test");
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger/index.html")).StatusCode);

        using var allowed = await SendPreflightAsync(client, "https://app.example.test");
        using var rejected = await SendPreflightAsync(client, "https://attacker.example.test");
        Assert.Equal("https://app.example.test", allowed.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(rejected.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static Task<HttpResponseMessage> SendPreflightAsync(HttpClient client, string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        return client.SendAsync(request);
    }
}
