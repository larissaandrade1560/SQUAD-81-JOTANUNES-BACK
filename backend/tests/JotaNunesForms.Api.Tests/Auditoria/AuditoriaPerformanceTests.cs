using System.Diagnostics;
using JotaNunesForms.Api.Tests.Infrastructure;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JotaNunesForms.Api.Tests.Auditoria;

[Collection(AuditoriaApiCollection.Name)]
public sealed class AuditoriaPerformanceTests(SecurityApiFactory factory)
{
    [Fact]
    [Trait("Category", "Performance")]
    public async Task FirstPageAcrossOneHundredThousandEventsHasAnIndexedPlanAndP95UnderThreeSeconds()
    {
        await AuditoriaTestData.SeedCanonicalAsync(factory);
        var companyId = Guid.NewGuid();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<JotaNunesFormsDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO auditoria_eventos_documentais (
                id, chave_negocio, codigo, ocorreu_em, ator_tipo, ator_usuario_id, ator_nome, ator_perfil,
                escopo, origem_tipo, documento_id, versao_id, versao_numero, versao_anterior_id,
                versao_anterior_numero, empresa_id, empresa_razao_social, funcionario_id, funcionario_nome,
                processo_id, item_checklist_id, tipo_documento_rotulo, motivo, comentario, valido_ate)
            SELECT gen_random_uuid(), 'performance:' || serie::text, 'DocumentoVencido',
                   now() - (serie * interval '1 second'), 'Sistema', NULL, 'Sistema', NULL,
                   'Empresa', 'DocumentoEmpresa', gen_random_uuid(), gen_random_uuid(), 1, NULL, NULL,
                   {companyId}, 'Empresa sintética', NULL, NULL, NULL, NULL, 'Documento de teste', NULL, NULL, NULL
            FROM generate_series(1, 100000) AS serie
            """);

        var repository = scope.ServiceProvider.GetRequiredService<IEventoAuditoriaDocumentoRepository>();
        var commandCounter = factory.DatabaseCommandCounter;
        var query = new EventoAuditoriaDocumentoQuery(
            null, null, null, companyId, EscopoAuditoriaDocumento.Empresa, 1, 50);
        _ = await repository.SearchAsync(query);

        var durations = new List<double>(10);
        for (var index = 0; index < 10; index++)
        {
            commandCounter.Reset();
            var timer = Stopwatch.StartNew();
            var result = await repository.SearchAsync(query);
            timer.Stop();
            Assert.Equal(100000, result.Total);
            Assert.Equal(50, result.Items.Count);
            Assert.Equal(2, commandCounter.ReaderCommands);
            durations.Add(timer.Elapsed.TotalMilliseconds);
        }

        durations.Sort();
        Assert.True(durations[^1] <= 3000, $"p95 page query was {durations[^1]:F0} ms.");

        await db.Database.OpenConnectionAsync();
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"EXPLAIN (ANALYZE, BUFFERS) SELECT id FROM auditoria_eventos_documentais WHERE empresa_id = '{companyId}' ORDER BY ocorreu_em DESC, id DESC LIMIT 50";
        var plan = new System.Text.StringBuilder();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            plan.AppendLine(reader.GetString(0));
        }

        Assert.Contains("ix_auditoria_eventos_documentais_empresa_data_id", plan.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
