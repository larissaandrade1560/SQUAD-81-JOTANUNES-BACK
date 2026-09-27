using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Api.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Auditoria;

public static class AuditoriaTestData
{
    public static Task<SecurityTestData> SeedCanonicalAsync(SecurityApiFactory factory) =>
        factory.SeedCanonicalDataAsync();

    public static EventoAuditoriaDocumento CreateCompanyEvent(
        SecurityTestData data,
        Guid companyId,
        string companyName,
        CodigoAuditoriaDocumento code,
        DateTime occurredAt,
        Guid? eventId = null,
        Guid? documentId = null,
        string? businessKey = null)
    {
        var versionId = Guid.NewGuid();
        return EventoAuditoriaDocumento.Create(
            businessKey ?? $"test:{Guid.NewGuid():N}",
            code,
            DateTime.SpecifyKind(occurredAt, DateTimeKind.Utc),
            code == CodigoAuditoriaDocumento.DocumentoVencido ? TipoAtorAuditoria.Sistema : TipoAtorAuditoria.Usuario,
            code == CodigoAuditoriaDocumento.DocumentoVencido ? null : data.Admin.Id,
            code == CodigoAuditoriaDocumento.DocumentoVencido ? "Sistema" : data.Admin.NomeExibicao,
            code == CodigoAuditoriaDocumento.DocumentoVencido ? null : data.Admin.PerfilRotulo,
            EscopoAuditoriaDocumento.Empresa,
            OrigemDocumentoAuditoria.DocumentoEmpresa,
            documentId ?? Guid.NewGuid(),
            versionId,
            1,
            null,
            null,
            companyId,
            companyName,
            null,
            null,
            null,
            null,
            "Documento de teste",
            code == CodigoAuditoriaDocumento.DocumentoRejeitado ? "Motivo de teste" : null,
            null,
            null,
            eventId);
    }

    public static async Task PersistAsync(SecurityApiFactory factory, params EventoAuditoriaDocumento[] events)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventoAuditoriaDocumentoRepository>();
        foreach (var evento in events)
        {
            await repository.AddAsync(evento);
        }
    }
}
