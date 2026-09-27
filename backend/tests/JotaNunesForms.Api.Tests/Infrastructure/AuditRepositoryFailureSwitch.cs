using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using JotaNunesForms.Infrastructure.Persistence.Repositories;

namespace JotaNunesForms.Api.Tests.Infrastructure;

public sealed class AuditRepositoryFailureSwitch
{
    private int _failWrites;

    public bool FailWrites
    {
        get => Volatile.Read(ref _failWrites) == 1;
        set => Volatile.Write(ref _failWrites, value ? 1 : 0);
    }
}

public sealed class SwitchableEventoAuditoriaDocumentoRepository : IEventoAuditoriaDocumentoRepository
{
    private readonly EventoAuditoriaDocumentoRepository _inner;
    private readonly AuditRepositoryFailureSwitch _failureSwitch;

    public SwitchableEventoAuditoriaDocumentoRepository(
        JotaNunesFormsDbContext db,
        AuditRepositoryFailureSwitch failureSwitch)
    {
        _inner = new EventoAuditoriaDocumentoRepository(db);
        _failureSwitch = failureSwitch;
    }

    public Task AddAsync(EventoAuditoriaDocumento evento, CancellationToken cancellationToken = default) =>
        _failureSwitch.FailWrites
            ? Task.FromException(new InvalidOperationException("Injected audit repository failure."))
            : _inner.AddAsync(evento, cancellationToken);

    public Task<(EventoAuditoriaDocumento Evento, bool Inserido)> AddOrGetByBusinessKeyAsync(
        EventoAuditoriaDocumento evento,
        CancellationToken cancellationToken = default) =>
        _failureSwitch.FailWrites
            ? Task.FromException<(EventoAuditoriaDocumento, bool)>(new InvalidOperationException("Injected audit repository failure."))
            : _inner.AddOrGetByBusinessKeyAsync(evento, cancellationToken);

    public Task<EventoAuditoriaDocumento?> GetByBusinessKeyAsync(
        string chaveNegocio,
        CancellationToken cancellationToken = default) =>
        _inner.GetByBusinessKeyAsync(chaveNegocio, cancellationToken);

    public Task<EventoAuditoriaDocumentoPage> SearchAsync(
        EventoAuditoriaDocumentoQuery query,
        CancellationToken cancellationToken = default) =>
        _inner.SearchAsync(query, cancellationToken);
}
