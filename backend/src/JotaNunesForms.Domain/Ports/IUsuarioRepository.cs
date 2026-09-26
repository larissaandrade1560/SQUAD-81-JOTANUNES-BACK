using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default);

    Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Usuario?> GetTerceirizadoByEmpresaAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    async Task<UsuarioSecurityState?> GetSecurityStateByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var usuario = await GetByIdAsync(id, cancellationToken);
        return usuario is null ? null : new UsuarioSecurityState(usuario, null, null);
    }

    Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsDocumentoAsync(
        string documento,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsEmailAsync(
        string email,
        Guid? excludeUsuarioId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Usuario usuario, CancellationToken cancellationToken = default);

    Task UpdateAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
