using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Api.Tests.Mobilizacoes;

namespace JotaNunesForms.Api.Tests.Mobilizacoes;

[Collection(MobilizacaoUs4ApiCollection.Name)]
public sealed class MovimentoEpiContractTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task Route_IsRegistered()
    {
        var data = await MobilizacaoUs4TestData.SeedCanonicalAsync(factory);
        await MobilizacaoUs4TestData.SeedAdmissionalChecklistAsync(factory, data);
        using var client = factory.CreateAuthenticatedClient(data.MoA);
        var response = await client.GetAsync($"/api/mobilizacoes/{data.MobilizationA.Id}/epi");
        Assert.NotEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}
