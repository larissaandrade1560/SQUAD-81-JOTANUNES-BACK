using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Auditoria;

[Collection(AuditoriaApiCollection.Name)]
public sealed class AuditoriaConsultaRepositoryTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task AppliesFiltersWithAndStableOrderingAndOriginAvailability()
    {
        var data = await AuditoriaTestData.SeedCanonicalAsync(factory);
        var companyId = Guid.NewGuid();
        var timestamp = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);
        var firstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        await AuditoriaTestData.PersistAsync(factory,
            AuditoriaTestData.CreateCompanyEvent(data, companyId, "Empresa filtro", CodigoAuditoriaDocumento.DocumentoAprovado, timestamp, firstId),
            AuditoriaTestData.CreateCompanyEvent(data, companyId, "Empresa filtro", CodigoAuditoriaDocumento.DocumentoAprovado, timestamp, secondId),
            AuditoriaTestData.CreateCompanyEvent(data, companyId, "Empresa filtro", CodigoAuditoriaDocumento.DocumentoRejeitado, timestamp.AddHours(-1)),
            AuditoriaTestData.CreateCompanyEvent(data, Guid.NewGuid(), "Outra empresa", CodigoAuditoriaDocumento.DocumentoAprovado, timestamp));

        await using var scope = factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventoAuditoriaDocumentoRepository>();
        var query = new EventoAuditoriaDocumentoQuery(
            timestamp.AddDays(-1), timestamp.AddDays(1), CodigoAuditoriaDocumento.DocumentoAprovado,
            companyId, EscopoAuditoriaDocumento.Empresa, 1, 1);
        var firstPage = await repository.SearchAsync(query);
        var secondPage = await repository.SearchAsync(query with { Page = 2 });

        Assert.Equal(2, firstPage.Total);
        Assert.Equal(secondId, firstPage.Items.Single().Evento.Id);
        Assert.Equal(firstId, secondPage.Items.Single().Evento.Id);
        Assert.False(firstPage.Items.Single().OrigemDisponivel);
        Assert.NotEqual(firstPage.Items[0].Evento.Id, secondPage.Items[0].Evento.Id);
    }
}
