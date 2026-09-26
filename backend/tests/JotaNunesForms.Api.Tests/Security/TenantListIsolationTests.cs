using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class TenantListIsolationTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task MoCompanyListsAreScopedToCurrentCompany()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.MoA);

        var companiesResponse = await client.GetAsync("/api/empresas");
        var companiesBody = await companiesResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, companiesResponse.StatusCode);
        Assert.Contains(data.CompanyA.Id.ToString(), companiesBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(data.CompanyB.Id.ToString(), companiesBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(data.MaterialsCompany.Id.ToString(), companiesBody, StringComparison.OrdinalIgnoreCase);

        var cases = new[]
        {
            (Path: "/api/documentos-empresa", OwnId: data.CompanyDocumentA.Id, ForeignId: data.CompanyDocumentB.Id),
            (Path: "/api/funcionarios", OwnId: data.EmployeeA.Id, ForeignId: data.EmployeeB.Id),
            (Path: "/api/contratos", OwnId: data.ContractA.Id, ForeignId: data.ContractB.Id),
            (Path: "/api/processos-contratacao", OwnId: data.ProcessA.Id, ForeignId: data.ProcessB.Id),
            (Path: "/api/mobilizacoes", OwnId: data.MobilizationA.Id, ForeignId: data.MobilizationB.Id),
            (Path: "/api/pagamentos", OwnId: data.PaymentA.Id, ForeignId: data.PaymentB.Id),
        };

        foreach (var testCase in cases)
        {
            var response = await client.GetAsync(testCase.Path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains(testCase.OwnId.ToString(), body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(testCase.ForeignId.ToString(), body, StringComparison.OrdinalIgnoreCase);
        }
    }
}
