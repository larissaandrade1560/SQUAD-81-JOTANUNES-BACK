using System.Net;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class AuthorizedRegressionTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task IntendedRoleAndTenantFlowsRemainAvailable()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var admin = factory.CreateAuthenticatedClient(data.Admin);
        using var analyst = factory.CreateAuthenticatedClient(data.Analyst);
        using var labor = factory.CreateAuthenticatedClient(data.MoA);
        using var materials = factory.CreateAuthenticatedClient(data.Materials);

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await analyst.GetAsync("/api/validacao/fila")).StatusCode);

        var laborEmployees = await labor.GetAsync("/api/funcionarios");
        Assert.Equal(HttpStatusCode.OK, laborEmployees.StatusCode);
        var laborProcesses = await labor.GetAsync("/api/processos-contratacao");
        Assert.Equal(HttpStatusCode.OK, laborProcesses.StatusCode);
        Assert.Contains(data.EmployeeA.Id.ToString(), await laborEmployees.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);

        var materialsDocuments = await materials.GetAsync("/api/documentos-empresa");
        Assert.Equal(HttpStatusCode.OK, materialsDocuments.StatusCode);
        Assert.Contains(data.CompanyDocumentB.Id.ToString(), await materialsDocuments.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.Forbidden, (await materials.GetAsync("/api/funcionarios")).StatusCode);
    }
}
