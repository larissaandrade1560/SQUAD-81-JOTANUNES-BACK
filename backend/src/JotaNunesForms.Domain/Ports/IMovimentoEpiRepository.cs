using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IMovimentoEpiRepository
{
    Task<MovimentoEpi?> GetByMobilizacaoAndIdempotencyKeyAsync(
        Guid mobilizacaoId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MovimentoEpi>> ListByMobilizacaoAsync(
        Guid mobilizacaoId,
        CancellationToken cancellationToken = default);

    Task AddAsync(MovimentoEpi movimento, CancellationToken cancellationToken = default);
}
