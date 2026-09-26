using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IMobilizacaoRepository
{
    Task<Mobilizacao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Mobilizacao>> ListAsync(
        Guid? empresaId = null,
        Guid? obraId = null,
        Guid? contratoId = null,
        SituacaoMobilizacao? situacao = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Mobilizacao mobilizacao, CancellationToken cancellationToken = default);

    Task UpdateAsync(Mobilizacao mobilizacao, CancellationToken cancellationToken = default);

    Task AddLotacaoAsync(HistoricoLotacao lotacao, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HistoricoLotacao>> ListLotacoesAsync(
        Guid mobilizacaoId,
        CancellationToken cancellationToken = default);
}
