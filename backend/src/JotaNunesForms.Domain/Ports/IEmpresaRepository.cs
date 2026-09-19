using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IEmpresaRepository
{
    Task<Empresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Empresa>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsCnpjAsync(
        string cnpj,
        Guid? excludeEmpresaId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Empresa empresa, CancellationToken cancellationToken = default);

    Task UpdateAsync(Empresa empresa, CancellationToken cancellationToken = default);
}
