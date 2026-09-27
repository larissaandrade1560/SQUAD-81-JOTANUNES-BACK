using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace JotaNunesForms.Api.Tests.Auditoria;

[Collection(AuditoriaApiCollection.Name)]
public sealed class AuditoriaImutabilidadeTests(SecurityApiFactory factory)
{
    [Fact]
    public async Task PostgreSqlTriggerRejectsUpdateAndDeleteAndKeepsTheEvent()
    {
        var data = await AuditoriaTestData.SeedCanonicalAsync(factory);
        var evento = AuditoriaTestData.CreateCompanyEvent(
            data, Guid.NewGuid(), "Empresa snapshot", JotaNunesForms.Domain.Entities.CodigoAuditoriaDocumento.DocumentoEnviado,
            DateTime.UtcNow);
        await AuditoriaTestData.PersistAsync(factory, evento);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
        var update = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE auditoria_eventos_documentais SET ator_nome = 'alterado' WHERE id = {evento.Id}"));
        Assert.Equal("55000", update.SqlState);

        var delete = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM auditoria_eventos_documentais WHERE id = {evento.Id}"));
        Assert.Equal("55000", delete.SqlState);

        var persisted = await scope.ServiceProvider.GetRequiredService<IEventoAuditoriaDocumentoRepository>()
            .GetByBusinessKeyAsync(evento.ChaveNegocio);
        Assert.NotNull(persisted);
        Assert.Equal(evento.AtorNome, persisted.AtorNome);
    }
}
