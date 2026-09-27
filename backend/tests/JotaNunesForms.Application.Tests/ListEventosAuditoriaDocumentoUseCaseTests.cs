using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.UseCases.Auditoria;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.Tests;

public sealed class ListEventosAuditoriaDocumentoUseCaseTests
{
    [Fact]
    public async Task ConvertsUtcFiltersAndMapsSnapshotAndUnavailableOrigin()
    {
        var evento = EventoAuditoriaDocumento.Create(
            "test:list-event", CodigoAuditoriaDocumento.DocumentoAprovado, DateTime.UtcNow,
            TipoAtorAuditoria.Usuario, Guid.NewGuid(), "Analista", "Analista",
            EscopoAuditoriaDocumento.Empresa, OrigemDocumentoAuditoria.DocumentoEmpresa,
            Guid.NewGuid(), Guid.NewGuid(), 2, null, null,
            Guid.NewGuid(), "Empresa de teste", null, null, null, null,
            "Contrato social", null, "Aprovado", null);
        var repository = new FakeEventRepository(new EventoAuditoriaDocumentoPage(
            [new EventoAuditoriaDocumentoSearchResult(evento, false)], 7));
        var useCase = new ListEventosAuditoriaDocumentoUseCase(repository);
        var from = new DateTimeOffset(2026, 9, 20, 3, 0, 0, TimeSpan.FromHours(-3));
        var to = new DateTimeOffset(2026, 9, 22, 3, 0, 0, TimeSpan.FromHours(-3));

        var response = await useCase.ExecuteAsync(new AuditoriaEventosRequest(
            from, to, "documento_aprovado", Guid.NewGuid(), "empresa", 2, 20));

        Assert.Equal(DateTime.SpecifyKind(from.UtcDateTime, DateTimeKind.Utc), repository.LastQuery!.De);
        Assert.Equal(CodigoAuditoriaDocumento.DocumentoAprovado, repository.LastQuery.Codigo);
        Assert.Equal(EscopoAuditoriaDocumento.Empresa, repository.LastQuery.Escopo);
        Assert.Equal(7, response.Total);
        Assert.Equal(1, response.TotalPages);
        Assert.Equal("documento_aprovado", response.Items[0].Codigo);
        Assert.Equal("Empresa de teste", response.Items[0].Empresa.RazaoSocial);
        Assert.False(response.Items[0].Origem.Disponivel);
        Assert.Equal(2, response.Items[0].Documento.VersaoNumero);
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task RejectsInvalidPaginationBeforeQuery(int page, int pageSize)
    {
        var repository = new FakeEventRepository(new EventoAuditoriaDocumentoPage([], 0));
        var useCase = new ListEventosAuditoriaDocumentoUseCase(repository);

        await Assert.ThrowsAsync<AuditoriaConsultaException>(() => useCase.ExecuteAsync(
            new AuditoriaEventosRequest(null, null, null, null, null, page, pageSize)));
        Assert.Null(repository.LastQuery);
    }

    [Fact]
    public async Task RejectsInvalidDateRangeAndUnknownFilters()
    {
        var repository = new FakeEventRepository(new EventoAuditoriaDocumentoPage([], 0));
        var useCase = new ListEventosAuditoriaDocumentoUseCase(repository);

        await Assert.ThrowsAsync<AuditoriaConsultaException>(() => useCase.ExecuteAsync(
            new AuditoriaEventosRequest(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null, null)));
        await Assert.ThrowsAsync<AuditoriaConsultaException>(() => useCase.ExecuteAsync(
            new AuditoriaEventosRequest(null, null, "unknown", null, null)));
        Assert.Null(repository.LastQuery);
    }

    private sealed class FakeEventRepository(EventoAuditoriaDocumentoPage page) : IEventoAuditoriaDocumentoRepository
    {
        public EventoAuditoriaDocumentoQuery? LastQuery { get; private set; }

        public Task AddAsync(EventoAuditoriaDocumento evento, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<(EventoAuditoriaDocumento Evento, bool Inserido)> AddOrGetByBusinessKeyAsync(
            EventoAuditoriaDocumento evento,
            CancellationToken cancellationToken = default) => Task.FromResult((evento, true));

        public Task<EventoAuditoriaDocumento?> GetByBusinessKeyAsync(
            string chaveNegocio,
            CancellationToken cancellationToken = default) => Task.FromResult<EventoAuditoriaDocumento?>(null);

        public Task<EventoAuditoriaDocumentoPage> SearchAsync(
            EventoAuditoriaDocumentoQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(page);
        }
    }
}
