using System.Net;
using System.Net.Http.Json;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Security;

/// <summary>RF19 — visão de conformidade por obra (somente equipe interna).</summary>
[Collection(SecurityApiCollection.Name)]
public sealed class ObraVisaoConformidadeApiTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task AnalystSeesAllocatedEmployeeGroupedByCompany()
    {
        var data = await factory.SeedCanonicalDataAsync();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var vinculos = scope.ServiceProvider.GetRequiredService<IFuncionarioObraRepository>();
            await vinculos.EnsureVinculoAsync(data.EmployeeA.Id, data.ResourceA.Id);
        }

        using var analyst = factory.CreateAuthenticatedClient(data.Analyst);
        var response = await analyst.GetAsync($"/api/obras/{data.ResourceA.Id}/visao-conformidade");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var visao = await response.Content.ReadFromJsonAsync<ObraVisaoConformidadeResponse>();
        Assert.NotNull(visao);
        Assert.Equal(data.ResourceA.Codigo, visao.Obra.Codigo);

        var empresa = Assert.Single(visao.Empresas, e => e.EmpresaId == data.CompanyA.Id);
        Assert.Equal(data.CompanyA.RazaoSocial, empresa.RazaoSocial);
        var funcionario = Assert.Single(empresa.Funcionarios, f => f.FuncionarioId == data.EmployeeA.Id);
        Assert.Equal(1, funcionario.Documentos.Total);
        Assert.NotEqual("sem_documentos", funcionario.Documentos.Situacao);
        Assert.Equal(1, funcionario.Pagamentos.Total);
        Assert.DoesNotContain(visao.Empresas, e => e.EmpresaId == data.CompanyB.Id);
    }

    [Fact]
    public async Task ObraWithoutAllocationsReturnsEmptyGroups()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var admin = factory.CreateAuthenticatedClient(data.Admin);

        var response = await admin.GetAsync($"/api/obras/{data.ResourceB.Id}/visao-conformidade");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var visao = await response.Content.ReadFromJsonAsync<ObraVisaoConformidadeResponse>();
        Assert.NotNull(visao);
        Assert.Equal(0, visao.TotalFuncionarios);
        Assert.Empty(visao.Empresas);
    }

    [Fact]
    public async Task UnknownObraReturnsNotFound()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var analyst = factory.CreateAuthenticatedClient(data.Analyst);

        var response = await analyst.GetAsync($"/api/obras/{Guid.NewGuid()}/visao-conformidade");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PartnerCannotReadAggregatedView()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var partner = factory.CreateAuthenticatedClient(data.MoA);

        var response = await partner.GetAsync($"/api/obras/{data.ResourceA.Id}/visao-conformidade");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
