using System.Net;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Api.Tests.Mobilizacoes;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(MobilizacaoUs4ApiCollection.Name)]
public sealed class MobilizacaoLiberacaoAuthorizationTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task AnonymousRequest_ReturnsUnauthorized()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateSecurityClient();
        var response = await client.GetAsync($"/api/mobilizacoes/{data.MobilizationA.Id}/liberacao");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MaterialsCompany_ReturnsForbidden()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.Materials);
        var response = await client.GetAsync($"/api/mobilizacoes/{data.MobilizationA.Id}/liberacao");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ForeignMoAndMissingMobilization_ReturnIdenticalNotFoundBodies()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.MoA);

        var foreign = await client.GetAsync($"/api/mobilizacoes/{data.MobilizationB.Id}/liberacao");
        var missing = await client.GetAsync($"/api/mobilizacoes/{Guid.NewGuid()}/liberacao");

        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(foreign.StatusCode, missing.StatusCode);
        Assert.Equal(await foreign.Content.ReadAsStringAsync(), await missing.Content.ReadAsStringAsync());
    }
}
