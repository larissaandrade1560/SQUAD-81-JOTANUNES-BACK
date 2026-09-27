using System.Net;
using System.Net.Http.Headers;
using System.Text;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Auditoria;

[Collection(AuditoriaApiCollection.Name)]
public sealed class AuditoriaAtomicidadeTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task AuditFailureRollsBackDocumentAndCompensatesTheUploadedObject()
    {
        var data = await AuditoriaTestData.SeedCanonicalAsync(factory);
        var beforeObjects = factory.Storage.ObjectCount;
        var beforeDeletes = factory.Storage.DeleteCount;
        int beforeDocuments;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            beforeDocuments = await scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>()
                .DocumentosEmpresa.CountAsync();
        }

        factory.AuditFailureSwitch.FailWrites = true;
        try
        {
            using var partner = factory.CreateAuthenticatedClient(data.MoA);
            using var form = new MultipartFormDataContent();
            using var file = new ByteArrayContent(Encoding.UTF8.GetBytes("%PDF-1.7 test"));
            file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(file, "arquivo", "rf17-atomicidade.pdf");
            form.Add(new StringContent(TipoDocumentoEmpresarial.Outro.ToString()), "tipo");

            var response = await partner.PostAsync("/api/documentos-empresa", form);
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
        finally
        {
            factory.AuditFailureSwitch.FailWrites = false;
        }

        Assert.Equal(beforeObjects, factory.Storage.ObjectCount);
        Assert.Equal(beforeDeletes + 1, factory.Storage.DeleteCount);
        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
        Assert.Equal(beforeDocuments, await verifyDb.DocumentosEmpresa.CountAsync());
        Assert.Equal(0, await verifyDb.DocumentosArquivosVersoes.CountAsync(version => version.NomeArquivo == "rf17-atomicidade.pdf"));
    }
}
