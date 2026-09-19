using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default);

    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsDocumentoAsync(
        string documento,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);

    Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
