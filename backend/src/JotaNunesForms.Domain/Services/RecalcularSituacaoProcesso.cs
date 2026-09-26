using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Services;

public static class RecalcularSituacaoProcesso
{
    public static SituacaoProcesso Calcular(
        SituacaoProcesso atual,
        IReadOnlyList<ItemChecklist> itens)
    {
        ArgumentNullException.ThrowIfNull(itens);

        if (atual == SituacaoProcesso.Encerrado)
        {
            return SituacaoProcesso.Encerrado;
        }

        var qualificacao = itens
            .Where(i => i.Ativo && i.Obrigatorio && i.TitularTipo != TitularRequisito.Trabalhador)
            .ToList();

        if (qualificacao.Count == 0)
        {
            return SituacaoProcesso.Aberto;
        }

        return qualificacao.All(i => i.Situacao == SituacaoItemChecklist.Aprovado)
            ? SituacaoProcesso.Qualificado
            : SituacaoProcesso.Aberto;
    }

    public static void AplicarVencimento(
        ItemChecklist item,
        DateTime? validoAte,
        DateTime utcNow,
        bool permiteVencerComoDocumento)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!permiteVencerComoDocumento || validoAte is null)
        {
            return;
        }

        if (item.Situacao == SituacaoItemChecklist.Aprovado && validoAte.Value.ToUniversalTime() < utcNow)
        {
            item.DefinirSituacao(SituacaoItemChecklist.Vencido);
        }
    }
}
