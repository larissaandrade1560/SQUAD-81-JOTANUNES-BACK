using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IObraRepository
{
    Task<Obra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Obra>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsCodigoAsync(
        string codigo,
        Guid? excludeObraId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Obra obra, CancellationToken cancellationToken = default);

    Task UpdateAsync(Obra obra, CancellationToken cancellationToken = default);
}
