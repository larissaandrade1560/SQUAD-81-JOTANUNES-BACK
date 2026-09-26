using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Services;

public sealed record ItemChecklistDraft(
    Guid CatalogoRequisitoId,
    string Codigo,
    TitularRequisito Titular,
    int? TitularOrdem,
    bool Obrigatorio);

public static class GeracaoChecklist
{
    public static IReadOnlyList<ItemChecklistDraft> Gerar(
        ProcessoContratacao processo,
        TipoEmpresa tipoEmpresa,
        IReadOnlyList<CatalogoRequisito> catalogo)
    {
        ArgumentNullException.ThrowIfNull(processo);
        ArgumentNullException.ThrowIfNull(catalogo);

        if (tipoEmpresa == TipoEmpresa.Materiais && processo.MobilizaTrabalhadores)
        {
            throw new InvalidOperationException(
                "Empresa de materiais não pode abrir processo com mobilização de trabalhadores.");
        }

        var drafts = new List<ItemChecklistDraft>();

        foreach (var requisito in catalogo.Where(r => r.Ativo))
        {
            if (requisito.Titular == TitularRequisito.Trabalhador)
            {
                continue;
            }

            if (!AplicaAoProcesso(requisito, processo))
            {
                continue;
            }

            if (requisito.Titular == TitularRequisito.Socio)
            {
                for (var ordem = 1; ordem <= processo.QuantidadeSociosInformada; ordem++)
                {
                    drafts.Add(new ItemChecklistDraft(
                        requisito.Id,
                        requisito.Codigo,
                        TitularRequisito.Socio,
                        ordem,
                        Obrigatorio: true));
                }

                continue;
            }

            drafts.Add(new ItemChecklistDraft(
                requisito.Id,
                requisito.Codigo,
                requisito.Titular,
                TitularOrdem: null,
                Obrigatorio: true));
        }

        return drafts;
    }

    public static IReadOnlyList<ItemChecklistDraft> GerarAdmissional(
        IReadOnlyList<CatalogoRequisito> catalogo)
    {
        ArgumentNullException.ThrowIfNull(catalogo);

        return catalogo
            .Where(r => r.Ativo && r.Titular == TitularRequisito.Trabalhador)
            .Select(r => new ItemChecklistDraft(
                r.Id,
                r.Codigo,
                TitularRequisito.Trabalhador,
                TitularOrdem: null,
                Obrigatorio: true))
            .ToList();
    }

    private static bool AplicaAoProcesso(CatalogoRequisito requisito, ProcessoContratacao processo)
    {
        if (requisito.Aplicacao == AplicacaoRequisito.Sempre)
        {
            return true;
        }

        return requisito.Condicao is { } condicao && processo.CondicaoAtendida(condicao);
    }
}
