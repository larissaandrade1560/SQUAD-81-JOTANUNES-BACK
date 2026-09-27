using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Api.Tests.Mobilizacoes;

[Collection(MobilizacaoUs4ApiCollection.Name)]
public sealed class LiberacaoMobilizacaoContractTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task GetLiberacao_ReturnsMinimizedPayloadWithStableCodes()
    {
        var data = await MobilizacaoUs4TestData.SeedCanonicalAsync(factory);
        await MobilizacaoUs4TestData.SeedAdmissionalChecklistAsync(factory, data);
        using var client = factory.CreateAuthenticatedClient(data.MoA);

        var response = await client.GetAsync($"/api/mobilizacoes/{data.MobilizationA.Id}/liberacao");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(data.MobilizationA.Id, root.GetProperty("mobilizacaoId").GetGuid());
        Assert.False(root.GetProperty("liberado").GetBoolean());
        Assert.True(root.TryGetProperty("impedimentos", out var impedimentos));
        Assert.True(impedimentos.GetArrayLength() > 0);
        Assert.Contains(
            impedimentos.EnumerateArray().Select(i => i.GetProperty("codigo").GetString()),
            code => code == CodigoImpedimentoLiberacao.IdentidadePendente);

        Assert.DoesNotContain("cpf", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("camposJson", json, StringComparison.OrdinalIgnoreCase);
    }
}
