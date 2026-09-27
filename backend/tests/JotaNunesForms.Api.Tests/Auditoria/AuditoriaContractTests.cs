using System.Net;
using System.Text.Json;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Auditoria;

[Collection(AuditoriaApiCollection.Name)]
public sealed class AuditoriaContractTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task DefaultsReturnTheOpenApiPageShape()
    {
        var data = await AuditoriaTestData.SeedCanonicalAsync(factory);
        using var analyst = factory.CreateAuthenticatedClient(data.Analyst);
        var response = await analyst.GetAsync("/api/auditoria/eventos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;
        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(50, root.GetProperty("pageSize").GetInt32());
        Assert.True(root.GetProperty("items").ValueKind == JsonValueKind.Array);
        Assert.True(root.GetProperty("total").GetInt64() >= 0);
        Assert.True(root.GetProperty("totalPages").GetInt32() >= 0);
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?pageSize=101")]
    [InlineData("?codigo=unknown")]
    [InlineData("?escopo=global")]
    [InlineData("?de=2026-09-22T00:00:00Z&ate=2026-09-21T00:00:00Z")]
    public async Task InvalidFilterValuesReturnBadRequest(string query)
    {
        var data = await AuditoriaTestData.SeedCanonicalAsync(factory);
        using var analyst = factory.CreateAuthenticatedClient(data.Analyst);
        var response = await analyst.GetAsync($"/api/auditoria/eventos{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("items", body, StringComparison.OrdinalIgnoreCase);
    }
}
