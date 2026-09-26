using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IConviteAcessoRepository
{
    Task<ConviteAcesso?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<ConviteAcesso?> GetLatestByEmpresaIdAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default);

    Task InvalidateUnusedForUsuarioAsync(
        Guid usuarioId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task AddAsync(ConviteAcesso convite, CancellationToken cancellationToken = default);

    Task UpdateAsync(ConviteAcesso convite, CancellationToken cancellationToken = default);
}
