using System.Net;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class MaterialCompanyAccessTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task MaterialsCompanyCannotAccessWorkforceModules()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.Materials);

        var operations = new (HttpMethod Method, string Path)[]
        {
            (HttpMethod.Get, "/api/funcionarios"),
            (HttpMethod.Post, "/api/funcionarios"),
            (HttpMethod.Put, $"/api/funcionarios/{Guid.NewGuid()}"),
            (HttpMethod.Get, $"/api/funcionarios/{Guid.NewGuid()}/documentos"),
            (HttpMethod.Post, $"/api/funcionarios/{Guid.NewGuid()}/documentos"),
            (HttpMethod.Post, $"/api/documentos-funcionario/{Guid.NewGuid()}/reenviar"),
            (HttpMethod.Get, $"/api/documentos-funcionario/{Guid.NewGuid()}/download"),
            (HttpMethod.Get, "/api/mobilizacoes"),
            (HttpMethod.Get, $"/api/mobilizacoes/{Guid.NewGuid()}"),
            (HttpMethod.Post, "/api/mobilizacoes"),
            (HttpMethod.Patch, $"/api/mobilizacoes/{Guid.NewGuid()}"),
            (HttpMethod.Get, "/api/pagamentos"),
            (HttpMethod.Post, "/api/pagamentos"),
            (HttpMethod.Post, $"/api/pagamentos/{Guid.NewGuid()}/comprovante"),
            (HttpMethod.Get, $"/api/pagamentos/{Guid.NewGuid()}/comprovante/download"),
        };

        foreach (var (method, path) in operations)
        {
            using var request = new HttpRequestMessage(method, path);
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        var companyDocuments = await client.GetAsync("/api/documentos-empresa");
        Assert.Equal(HttpStatusCode.OK, companyDocuments.StatusCode);
    }
}
