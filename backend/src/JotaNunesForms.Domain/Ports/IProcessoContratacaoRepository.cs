using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IProcessoContratacaoRepository
{
    Task<ProcessoContratacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProcessoContratacao>> ListAsync(
        Guid? empresaId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(ProcessoContratacao processo, CancellationToken cancellationToken = default);

    Task UpdateAsync(ProcessoContratacao processo, CancellationToken cancellationToken = default);
}
