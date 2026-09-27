using System.Net;
using System.Net.Http.Json;
using System.Text;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Security;

[Collection(SecurityApiCollection.Name)]
public sealed class TenantMutationSafetyTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task CrossTenantMutationsDoNotChangeDatabaseOrObjectStorage()
    {
        var data = await factory.SeedCanonicalDataAsync();
        using var client = factory.CreateAuthenticatedClient(data.MoA);
        factory.Storage.ResetCounters();

        var employeeUpdate = await client.PutAsJsonAsync(
            $"/api/funcionarios/{data.EmployeeB.Id}",
            new UpdateFuncionarioRequest("Nome alterado", "Cargo alterado", false));
        var missingEmployeeUpdate = await client.PutAsJsonAsync(
            $"/api/funcionarios/{Guid.NewGuid()}",
            new UpdateFuncionarioRequest("Nome alterado", "Cargo alterado", false));
        Assert.Equal(HttpStatusCode.NotFound, employeeUpdate.StatusCode);
        Assert.Equal(await employeeUpdate.Content.ReadAsStringAsync(), await missingEmployeeUpdate.Content.ReadAsStringAsync());

        var socioUpdate = await client.PutAsJsonAsync(
            $"/api/empresas/{data.CompanyA.Id}/socios/{data.SocioB.Id}",
            new { nome = "Sócio alterado", ativo = false });
        var missingSocioUpdate = await client.PutAsJsonAsync(
            $"/api/empresas/{data.CompanyA.Id}/socios/{Guid.NewGuid()}",
            new { nome = "Sócio alterado", ativo = false });
        Assert.Equal(HttpStatusCode.NotFound, socioUpdate.StatusCode);
        Assert.Equal(await socioUpdate.Content.ReadAsStringAsync(), await missingSocioUpdate.Content.ReadAsStringAsync());

        using var checklistUpload = CreatePdfUpload();
        var checklistResponse = await client.PostAsync(
            $"/api/checklist-itens/{data.ChecklistB.Id}/versoes",
            checklistUpload);
        using var missingChecklistUpload = CreatePdfUpload();
        var missingChecklistResponse = await client.PostAsync(
            $"/api/checklist-itens/{Guid.NewGuid()}/versoes",
            missingChecklistUpload);
        Assert.Equal(HttpStatusCode.NotFound, checklistResponse.StatusCode);
        Assert.Equal(await checklistResponse.Content.ReadAsStringAsync(), await missingChecklistResponse.Content.ReadAsStringAsync());

        using var proofUpload = CreatePdfUpload();
        var proofResponse = await client.PostAsync(
            $"/api/pagamentos/{data.PaymentB.Id}/comprovante",
            proofUpload);
        using var missingProofUpload = CreatePdfUpload();
        var missingProofResponse = await client.PostAsync(
            $"/api/pagamentos/{Guid.NewGuid()}/comprovante",
            missingProofUpload);
        Assert.Equal(HttpStatusCode.NotFound, proofResponse.StatusCode);
        Assert.Equal(await proofResponse.Content.ReadAsStringAsync(), await missingProofResponse.Content.ReadAsStringAsync());

        using var documentUpload = CreatePdfUpload();
        var documentResponse = await client.PostAsync(
            $"/api/documentos-empresa/{data.CompanyDocumentB.Id}/reenviar",
            documentUpload);
        using var missingDocumentUpload = CreatePdfUpload();
        var missingDocumentResponse = await client.PostAsync(
            $"/api/documentos-empresa/{Guid.NewGuid()}/reenviar",
            missingDocumentUpload);
        Assert.Equal(HttpStatusCode.NotFound, documentResponse.StatusCode);
        Assert.Equal(await documentResponse.Content.ReadAsStringAsync(), await missingDocumentResponse.Content.ReadAsStringAsync());
        Assert.Equal(0, factory.Storage.UploadCount);

        var epiPayload = new StringContent(
            """
            {"tipo":1,"epi":"Capacete","quantidade":1,"numeroCa":"12345","data":"2026-09-27","orientacaoUso":true,"responsabilidadeGuarda":true,"aceiteTrabalhador":true}
            """,
            Encoding.UTF8,
            "application/json");
        using var epiRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/mobilizacoes/{data.MobilizationB.Id}/epi")
        {
            Content = epiPayload,
        };
        epiRequest.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString("N"));
        var crossTenantEpi = await client.SendAsync(epiRequest);
        Assert.Equal(HttpStatusCode.NotFound, crossTenantEpi.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
        var unchangedEmployee = await db.Funcionarios.AsNoTracking()
            .SingleAsync(employee => employee.Id == data.EmployeeB.Id);
        var unchangedSocio = await db.Socios.AsNoTracking()
            .SingleAsync(socio => socio.Id == data.SocioB.Id);
        var unchangedPayment = await db.PagamentosFuncionario.AsNoTracking()
            .SingleAsync(payment => payment.Id == data.PaymentB.Id);
        var unchangedDocument = await db.DocumentosEmpresa.AsNoTracking()
            .SingleAsync(document => document.Id == data.CompanyDocumentB.Id);

        Assert.Equal("Funcionário B", unchangedEmployee.Nome);
        Assert.True(unchangedEmployee.Ativo);
        Assert.Equal("Sócio B", unchangedSocio.Nome);
        Assert.True(unchangedSocio.Ativo);
        Assert.False(unchangedPayment.PossuiComprovante);
        Assert.Equal("company-b.pdf", unchangedDocument.NomeArquivo);
        Assert.Equal(1, await db.DocumentosVersoes.CountAsync(version => version.ItemChecklistId == data.ChecklistB.Id));
    }

    private static MultipartFormDataContent CreatePdfUpload()
    {
        var content = new MultipartFormDataContent();
        var file = new StreamContent(new MemoryStream(Encoding.ASCII.GetBytes("not-a-real-pdf")));
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(file, "arquivo", "security-test.pdf");
        return content;
    }
}
