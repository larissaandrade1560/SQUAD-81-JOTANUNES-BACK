using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Mobilizacoes;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Application.UseCases.Mobilizacoes;

public sealed class RecalcularLiberacaoMobilizacaoUseCase
{
    private readonly IMobilizacaoRepository _mobilizacoes;
    private readonly IItemChecklistRepository _itens;
    private readonly ICatalogoRequisitoRepository _catalogo;
    private readonly IMovimentoEpiRepository _movimentosEpi;
    private readonly IIntegracaoObraRepository _integracoes;
    private readonly LiberacaoSnapshotBuilder _snapshotBuilder;
    private readonly ITransactionalExecutor _transactions;

    public RecalcularLiberacaoMobilizacaoUseCase(
        IMobilizacaoRepository mobilizacoes,
        IItemChecklistRepository itens,
        ICatalogoRequisitoRepository catalogo,
        IMovimentoEpiRepository movimentosEpi,
        IIntegracaoObraRepository integracoes,
        LiberacaoSnapshotBuilder snapshotBuilder,
        ITransactionalExecutor transactions)
    {
        _mobilizacoes = mobilizacoes;
        _itens = itens;
        _catalogo = catalogo;
        _movimentosEpi = movimentosEpi;
        _integracoes = integracoes;
        _snapshotBuilder = snapshotBuilder;
        _transactions = transactions;
    }

    public Task<ResultadoLiberacao> ExecuteAsync(
        Guid mobilizacaoId,
        Funcionario funcionario,
        CancellationToken cancellationToken = default) =>
        _transactions.ExecuteAsync(
            async ct =>
            {
                var avaliadoEm = DateTime.UtcNow;
                var mobilizacao = await _mobilizacoes.GetByIdForUpdateAsync(mobilizacaoId, ct)
                    ?? throw new InvalidOperationException("Mobilização não encontrada.");
                var itens = await _itens.ListActiveWorkerItemsByMobilizacaoAsync(
                    mobilizacao.Id,
                    mobilizacao.ProcessoId,
                    ct);
                var catalogo = await _catalogo.ListAsync(ct);
                var codigos = catalogo.ToDictionary(c => c.Id, c => c.Codigo);

                await SincronizarItensDerivadosAsync(mobilizacao, funcionario, itens, codigos, avaliadoEm, ct);

                var contexto = await _snapshotBuilder.BuildAsync(mobilizacao, funcionario, itens, avaliadoEm, ct);
                var resultado = RegraLiberacaoTrabalhador.Avaliar(contexto);

                if (mobilizacao.Situacao != resultado.Situacao
                    && mobilizacao.Situacao is SituacaoMobilizacao.Aguardando or SituacaoMobilizacao.Liberado)
                {
                    mobilizacao.DefinirSituacao(resultado.Situacao);
                    await _mobilizacoes.UpdateAsync(mobilizacao, ct);
                }

                foreach (var item in itens)
                {
                    await _itens.UpdateAsync(item, ct);
                }

                return resultado;
            },
            cancellationToken);

    private async Task SincronizarItensDerivadosAsync(
        Mobilizacao mobilizacao,
        Funcionario funcionario,
        IReadOnlyList<ItemChecklist> itens,
        IReadOnlyDictionary<Guid, string> codigos,
        DateTime avaliadoEmUtc,
        CancellationToken cancellationToken)
    {
        var cadastroCompleto = MobilizacaoAccessService.CadastroCompleto(mobilizacao, funcionario);
        var movimentos = await _movimentosEpi.ListByMobilizacaoAsync(mobilizacao.Id, cancellationToken);
        var saldo = SaldoEpi.CalcularSaldoAtivo(movimentos);
        var integracoes = await _integracoes.ListByMobilizacaoAsync(mobilizacao.Id, cancellationToken);
        var vigente = integracoes.FirstOrDefault();

        foreach (var item in itens)
        {
            if (!codigos.TryGetValue(item.CatalogoRequisitoId, out var codigo))
            {
                continue;
            }

            switch (codigo)
            {
                case CatalogoRequisitoCodigos.MobCadastro:
                    item.DefinirSituacao(cadastroCompleto
                        ? SituacaoItemChecklist.Aprovado
                        : SituacaoItemChecklist.NaoEnviado);
                    break;
                case CatalogoRequisitoCodigos.EpiEntrega:
                    item.DefinirSituacao(saldo > 0
                        ? SituacaoItemChecklist.Aprovado
                        : SituacaoItemChecklist.NaoEnviado);
                    break;
                case CatalogoRequisitoCodigos.IntegracaoObra:
                    if (vigente is null)
                    {
                        item.DefinirSituacao(SituacaoItemChecklist.NaoEnviado);
                    }
                    else if (vigente.Refazer)
                    {
                        item.DefinirSituacao(SituacaoItemChecklist.NaoEnviado);
                    }
                    else if (vigente.ValidoAte is DateTime limite && limite.ToUniversalTime() <= avaliadoEmUtc)
                    {
                        item.DefinirSituacao(SituacaoItemChecklist.Vencido);
                    }
                    else if (vigente.EstaVigenteEm(avaliadoEmUtc))
                    {
                        item.DefinirSituacao(SituacaoItemChecklist.Aprovado);
                    }
                    else
                    {
                        item.DefinirSituacao(SituacaoItemChecklist.NaoEnviado);
                    }

                    break;
            }
        }
    }
}

public static class LiberacaoMapper
{
    public static ResultadoLiberacaoResponse ToResponse(ResultadoLiberacao resultado) =>
        new(
            resultado.MobilizacaoId,
            resultado.Situacao,
            resultado.Liberado,
            resultado.AvaliadoEm,
            resultado.Impedimentos
                .Select(i => new ImpedimentoLiberacaoResponse(i.Codigo, i.RequisitoCodigo, i.Motivo, i.ItemChecklistId))
                .ToList());
}
