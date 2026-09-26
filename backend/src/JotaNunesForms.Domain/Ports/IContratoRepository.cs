using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IContratoRepository
{
    Task<Contrato?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Contrato>> ListAsync(
        Guid? empresaId = null,
        Guid? obraId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Contrato contrato, CancellationToken cancellationToken = default);
}
