using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IIntegracaoObraRepository
{
    Task<IntegracaoObra?> GetByMobilizacaoAndIdempotencyKeyAsync(
        Guid mobilizacaoId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IntegracaoObra>> ListByMobilizacaoAsync(
        Guid mobilizacaoId,
        CancellationToken cancellationToken = default);

    Task AddAsync(IntegracaoObra integracao, CancellationToken cancellationToken = default);

    Task UpdateAsync(IntegracaoObra integracao, CancellationToken cancellationToken = default);
}
