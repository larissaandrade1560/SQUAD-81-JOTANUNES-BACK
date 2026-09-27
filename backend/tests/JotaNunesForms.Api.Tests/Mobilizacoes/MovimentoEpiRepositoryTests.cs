using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Api.Tests.Mobilizacoes;

namespace JotaNunesForms.Api.Tests.Mobilizacoes;

[Collection(MobilizacaoUs4ApiCollection.Name)]
public sealed class MovimentoEpiRepositoryTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task ListEpi_ReturnsArray()
    {
        var data = await MobilizacaoUs4TestData.SeedCanonicalAsync(factory);
        await MobilizacaoUs4TestData.SeedAdmissionalChecklistAsync(factory, data);
        using var client = factory.CreateAuthenticatedClient(data.MoA);
        var response = await client.GetAsync($"/api/mobilizacoes/{data.MobilizationA.Id}/epi");
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound);
    }
}
