using System.Net;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Api.Tests.Auditoria;

[Collection(AuditoriaApiCollection.Name)]
public sealed class AuditoriaSanitizacaoTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task AuditResponseAndLogsContainOnlyAllowlistedSnapshots()
    {
        var data = await AuditoriaTestData.SeedCanonicalAsync(factory);
        var evento = AuditoriaTestData.CreateCompanyEvent(
            data,
            data.CompanyA.Id,
            data.CompanyA.RazaoSocial,
            CodigoAuditoriaDocumento.DocumentoEnviado,
            DateTime.UtcNow,
            documentId: data.CompanyDocumentA.Id);
        await AuditoriaTestData.PersistAsync(factory, evento);
        using var admin = factory.CreateAuthenticatedClient(data.Admin);

        var response = await admin.GetAsync("/api/auditoria/eventos?empresaId=" + data.CompanyA.Id);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Empresa MO A", body, StringComparison.Ordinal);
        Assert.DoesNotContain("11111111111111", body, StringComparison.Ordinal);
        Assert.DoesNotContain("company-a/key", body, StringComparison.Ordinal);
        Assert.DoesNotContain("company-a.pdf", body, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer ", body, StringComparison.OrdinalIgnoreCase);

        var logs = string.Join('\n', factory.SecurityLogs.Messages);
        Assert.DoesNotContain("company-a/key", logs, StringComparison.Ordinal);
        Assert.DoesNotContain("11111111111111", logs, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer ", logs, StringComparison.OrdinalIgnoreCase);
    }
}
