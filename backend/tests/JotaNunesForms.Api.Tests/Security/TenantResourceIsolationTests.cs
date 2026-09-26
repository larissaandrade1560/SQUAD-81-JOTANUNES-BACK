using System.Net;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class TenantResourceIsolationTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task ForeignCompanyAndMissingCompanyHaveTheSameNotFoundResponse()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.MoA);

        var foreign = await client.GetAsync($"/api/empresas/{data.CompanyB.Id}");
        var missing = await client.GetAsync($"/api/empresas/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(foreign.StatusCode, missing.StatusCode);
        Assert.Equal(await foreign.Content.ReadAsStringAsync(), await missing.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ForeignTenantResourcesAndMissingResourcesHaveTheSameNotFoundResponse()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.MoA);
        factory.Storage.ResetCounters();

        var paths = new[]
        {
            ($"/api/documentos-empresa/{data.CompanyDocumentB.Id}/download", $"/api/documentos-empresa/{Guid.NewGuid()}/download"),
            ($"/api/empresas/{data.CompanyB.Id}/socios", $"/api/empresas/{Guid.NewGuid()}/socios"),
            ($"/api/funcionarios/{data.EmployeeB.Id}/documentos", $"/api/funcionarios/{Guid.NewGuid()}/documentos"),
            ($"/api/documentos-funcionario/{data.EmployeeDocumentB.Id}/download", $"/api/documentos-funcionario/{Guid.NewGuid()}/download"),
            ($"/api/checklist-itens/{data.ChecklistB.Id}/versoes", $"/api/checklist-itens/{Guid.NewGuid()}/versoes"),
            ($"/api/processos-contratacao/{data.ProcessB.Id}", $"/api/processos-contratacao/{Guid.NewGuid()}"),
            ($"/api/processos-contratacao/{data.ProcessB.Id}/checklist", $"/api/processos-contratacao/{Guid.NewGuid()}/checklist"),
            ($"/api/documentos-versoes/{data.VersionB.Id}/download", $"/api/documentos-versoes/{Guid.NewGuid()}/download"),
            ($"/api/mobilizacoes/{data.MobilizationB.Id}", $"/api/mobilizacoes/{Guid.NewGuid()}"),
            ($"/api/pagamentos/{data.PaymentB.Id}/comprovante/download", $"/api/pagamentos/{Guid.NewGuid()}/comprovante/download"),
        };

        foreach (var (foreignPath, missingPath) in paths)
        {
            var foreign = await client.GetAsync(foreignPath);
            var missing = await client.GetAsync(missingPath);

            Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
            Assert.Equal(foreign.StatusCode, missing.StatusCode);
            Assert.Equal(await foreign.Content.ReadAsStringAsync(), await missing.Content.ReadAsStringAsync());
        }

        Assert.Equal(0, factory.Storage.DownloadCount);
    }
}
