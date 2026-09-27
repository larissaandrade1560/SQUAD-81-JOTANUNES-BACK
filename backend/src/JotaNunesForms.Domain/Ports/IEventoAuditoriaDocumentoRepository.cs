using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IEventoAuditoriaDocumentoRepository
{
    Task AddAsync(EventoAuditoriaDocumento evento, CancellationToken cancellationToken = default);

    Task<(EventoAuditoriaDocumento Evento, bool Inserido)> AddOrGetByBusinessKeyAsync(
        EventoAuditoriaDocumento evento,
        CancellationToken cancellationToken = default);

    Task<EventoAuditoriaDocumento?> GetByBusinessKeyAsync(
        string chaveNegocio,
        CancellationToken cancellationToken = default);

    Task<EventoAuditoriaDocumentoPage> SearchAsync(
        EventoAuditoriaDocumentoQuery query,
        CancellationToken cancellationToken = default);
}

public sealed record EventoAuditoriaDocumentoQuery(
    DateTime? De,
    DateTime? Ate,
    CodigoAuditoriaDocumento? Codigo,
    Guid? EmpresaId,
    EscopoAuditoriaDocumento? Escopo,
    int Page,
    int PageSize);

public sealed record EventoAuditoriaDocumentoPage(
    IReadOnlyList<EventoAuditoriaDocumentoSearchResult> Items,
    long Total);

public sealed record EventoAuditoriaDocumentoSearchResult(
    EventoAuditoriaDocumento Evento,
    bool OrigemDisponivel);
