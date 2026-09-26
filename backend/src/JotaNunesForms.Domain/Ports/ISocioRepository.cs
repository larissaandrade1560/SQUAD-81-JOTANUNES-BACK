using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface ISocioRepository
{
    Task<Socio?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Socio>> ListByEmpresaAsync(Guid empresaId, CancellationToken cancellationToken = default);

    Task<bool> ExistsCpfAsync(Guid empresaId, string cpf, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Socio socio, CancellationToken cancellationToken = default);

    Task UpdateAsync(Socio socio, CancellationToken cancellationToken = default);
}
