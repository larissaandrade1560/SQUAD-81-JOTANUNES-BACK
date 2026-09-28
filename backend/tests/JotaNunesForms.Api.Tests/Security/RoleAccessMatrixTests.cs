using System.Net;
using JotaNunesForms.Api.Tests.Infrastructure;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class RoleAccessMatrixTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task AdministrativeAndInternalEndpointsEnforceCurrentRole()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var admin = factory.CreateAuthenticatedClient(data.Admin);
        using var analyst = factory.CreateAuthenticatedClient(data.Analyst);
        using var partner = factory.CreateAuthenticatedClient(data.MoA);

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await analyst.GetAsync("/api/usuarios")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.GetAsync("/api/usuarios")).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await analyst.GetAsync("/api/validacao")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await partner.GetAsync("/api/validacao")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await partner.GetAsync("/api/formularios")).StatusCode);
    }

    [Fact]
    public async Task ReadOperationsFollowTheRoleAndCompanyTypeMatrix()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var admin = factory.CreateAuthenticatedClient(data.Admin);
        using var analyst = factory.CreateAuthenticatedClient(data.Analyst);
        using var labor = factory.CreateAuthenticatedClient(data.MoA);
        using var materials = factory.CreateAuthenticatedClient(data.Materials);

        var operations = new (string Path, HttpStatusCode Admin, HttpStatusCode Analyst, HttpStatusCode Labor, HttpStatusCode Materials)[]
        {
            ("/api/usuarios", HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Forbidden, HttpStatusCode.Forbidden),
            ("/api/validacao", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Forbidden),
            ("/api/auditoria/eventos", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Forbidden),
            ("/api/formularios", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK),
            ("/api/empresas", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK),
            ("/api/obras", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK),
            ("/api/documentos-empresa", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK),
            ("/api/funcionarios", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Forbidden),
            ("/api/processos-contratacao", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK),
            ("/api/mobilizacoes", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Forbidden),
            ("/api/pagamentos", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Forbidden),
            ($"/api/obras/{data.ResourceA.Id}/visao-conformidade", HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.Forbidden),
        };

        foreach (var operation in operations)
        {
            Assert.Equal(operation.Admin, (await admin.GetAsync(operation.Path)).StatusCode);
            Assert.Equal(operation.Analyst, (await analyst.GetAsync(operation.Path)).StatusCode);
            Assert.Equal(operation.Labor, (await labor.GetAsync(operation.Path)).StatusCode);
            Assert.Equal(operation.Materials, (await materials.GetAsync(operation.Path)).StatusCode);
        }
    }
}
