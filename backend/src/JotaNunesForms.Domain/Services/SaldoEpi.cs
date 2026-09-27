using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Services;

public static class SaldoEpi
{
    public static int CalcularSaldoAtivo(IReadOnlyList<MovimentoEpi> movimentos)
    {
        ArgumentNullException.ThrowIfNull(movimentos);

        var saldoPorOrigem = new Dictionary<Guid, int>();
        var possuiEntrega = false;

        foreach (var movimento in movimentos.OrderBy(m => m.CriadoEm).ThenBy(m => m.Id))
        {
            switch (movimento.Tipo)
            {
                case TipoMovimentoEpi.Entrega:
                    possuiEntrega = true;
                    saldoPorOrigem[movimento.Id] = movimento.Quantidade;
                    break;
                case TipoMovimentoEpi.Substituicao:
                    if (movimento.MovimentoOrigemId is not Guid origemId
                        || !saldoPorOrigem.TryGetValue(origemId, out var saldoOrigem)
                        || saldoOrigem <= 0)
                    {
                        throw new InvalidOperationException("Substituição exige movimento de origem ativo.");
                    }

                    saldoPorOrigem[origemId] = 0;
                    saldoPorOrigem[movimento.Id] = saldoOrigem;
                    break;
                case TipoMovimentoEpi.Devolucao:
                    if (movimento.MovimentoOrigemId is not Guid origemDevolucao
                        || !saldoPorOrigem.TryGetValue(origemDevolucao, out var saldoDevolucao))
                    {
                        throw new InvalidOperationException("Devolução exige movimento de origem válido.");
                    }

                    if (movimento.Quantidade > saldoDevolucao)
                    {
                        throw new InvalidOperationException("Devolução não pode exceder o saldo do movimento de origem.");
                    }

                    saldoPorOrigem[origemDevolucao] = saldoDevolucao - movimento.Quantidade;
                    break;
            }
        }

        return possuiEntrega ? saldoPorOrigem.Values.Sum() : 0;
    }
}
