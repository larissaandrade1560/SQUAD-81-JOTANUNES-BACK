using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Auditoria;

[Collection(AuditoriaApiCollection.Name)]
public sealed class AuditoriaConcorrenciaTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task ConcurrentDuplicateTransitionCreatesOneAppendOnlyEvent()
    {
        var data = await AuditoriaTestData.SeedCanonicalAsync(factory);
        const string businessKey = "test:concurrent-document-transition";
        var occurredAt = DateTime.UtcNow;
        var attempts = Enumerable.Range(0, 8)
            .Select(index => AuditoriaTestData.CreateCompanyEvent(
                data,
                Guid.NewGuid(),
                "Empresa concorrente",
                CodigoAuditoriaDocumento.DocumentoEnviado,
                occurredAt,
                eventId: Guid.NewGuid(),
                businessKey: businessKey))
            .ToArray();

        var results = await Task.WhenAll(attempts.Select(async evento =>
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEventoAuditoriaDocumentoRepository>();
            return await repository.AddOrGetByBusinessKeyAsync(evento);
        }));

        Assert.Single(results.Where(result => result.Inserido));
        Assert.Single(results.Select(result => result.Evento.Id).Distinct());
        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyRepository = verifyScope.ServiceProvider.GetRequiredService<IEventoAuditoriaDocumentoRepository>();
        var stored = await verifyRepository.GetByBusinessKeyAsync(businessKey);
        Assert.NotNull(stored);
        Assert.Equal(results[0].Evento.Id, stored.Id);
    }
}
