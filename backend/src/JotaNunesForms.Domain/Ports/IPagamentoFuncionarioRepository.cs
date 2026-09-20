using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IPagamentoFuncionarioRepository
{
    Task<IReadOnlyList<PagamentoFuncionario>> ListAsync(
        Guid? empresaId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsCompetenciaAsync(
        Guid funcionarioId,
        DateOnly competencia,
        CancellationToken cancellationToken = default);

    Task AddAsync(PagamentoFuncionario pagamento, CancellationToken cancellationToken = default);
}
