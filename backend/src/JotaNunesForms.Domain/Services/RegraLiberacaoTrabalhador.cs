using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Services;

public static class CodigoImpedimentoLiberacao
{
    public const string CadastroIncompleto = "CADASTRO_INCOMPLETO";
    public const string IdentidadePendente = "IDENTIDADE_PENDENTE";
    public const string VinculoPendente = "VINCULO_PENDENTE";
    public const string VinculoPreliminarVencido = "VINCULO_PRELIMINAR_VENCIDO";
    public const string AsoPendente = "ASO_PENDENTE";
    public const string AsoInapto = "ASO_INAPTO";
    public const string AsoVencido = "ASO_VENCIDO";
    public const string Nr18Pendente = "NR18_PENDENTE";
    public const string Nr18Vencida = "NR18_VENCIDA";
    public const string OrdemServicoPendente = "ORDEM_SERVICO_PENDENTE";
    public const string EpiSemEntregaAtiva = "EPI_SEM_ENTREGA_ATIVA";
    public const string IntegracaoPendente = "INTEGRACAO_PENDENTE";
    public const string IntegracaoVencida = "INTEGRACAO_VENCIDA";
    public const string IntegracaoRefazer = "INTEGRACAO_REFAZER";
    public const string RequisitoAdicionalPendente = "REQUISITO_ADICIONAL_PENDENTE";
}

public sealed record ImpedimentoLiberacao(
    string Codigo,
    string RequisitoCodigo,
    string Motivo,
    Guid? ItemChecklistId = null);

public sealed record ResultadoLiberacao(
    Guid MobilizacaoId,
    SituacaoMobilizacao Situacao,
    bool Liberado,
    DateTime AvaliadoEm,
    IReadOnlyList<ImpedimentoLiberacao> Impedimentos);

public sealed record RequisitoLiberacaoEntrada(
    string Codigo,
    Guid? ItemChecklistId,
    SituacaoItemChecklist? Situacao,
    bool AsoInapto = false);

public sealed record ContextoLiberacaoTrabalhador(
    Guid MobilizacaoId,
    SituacaoMobilizacao SituacaoAtual,
    bool CadastroCompleto,
    IReadOnlyList<RequisitoLiberacaoEntrada> Requisitos,
    int SaldoEpiAtivo,
    bool IntegracaoRefazer,
    bool IntegracaoVencida,
    bool IntegracaoValida,
    DateTime AvaliadoEmUtc);

public static class RegraLiberacaoTrabalhador
{
    private static readonly string[] OrdemPadrao =
    [
        CatalogoRequisitoCodigos.MobCadastro,
        CatalogoRequisitoCodigos.DocOficialFoto,
        CatalogoRequisitoCodigos.EsocialVinculo,
        CatalogoRequisitoCodigos.AsoAdmissional,
        CatalogoRequisitoCodigos.Nr18Basica,
        CatalogoRequisitoCodigos.OrdemServico,
        CatalogoRequisitoCodigos.EpiEntrega,
        CatalogoRequisitoCodigos.IntegracaoObra,
    ];

    private static readonly HashSet<string> CodigosPadrao = OrdemPadrao.ToHashSet(StringComparer.Ordinal);

    public static ResultadoLiberacao Avaliar(ContextoLiberacaoTrabalhador contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);

        var impedimentos = new List<ImpedimentoLiberacao>();

        if (!contexto.CadastroCompleto)
        {
            impedimentos.Add(Criar(
                CodigoImpedimentoLiberacao.CadastroIncompleto,
                CatalogoRequisitoCodigos.MobCadastro,
                "Cadastro da mobilização incompleto.",
                Requisito(contexto, CatalogoRequisitoCodigos.MobCadastro)?.ItemChecklistId));
        }

        foreach (var codigo in OrdemPadrao.Where(c => c != CatalogoRequisitoCodigos.MobCadastro))
        {
            if (codigo == CatalogoRequisitoCodigos.EpiEntrega)
            {
                if (contexto.SaldoEpiAtivo <= 0)
                {
                    impedimentos.Add(Criar(
                        CodigoImpedimentoLiberacao.EpiSemEntregaAtiva,
                        codigo,
                        "Não há entrega ativa de EPI para a mobilização.",
                        Requisito(contexto, codigo)?.ItemChecklistId));
                }

                continue;
            }

            if (codigo == CatalogoRequisitoCodigos.IntegracaoObra)
            {
                AdicionarIntegracao(contexto, impedimentos);
                continue;
            }

            var requisito = Requisito(contexto, codigo);
            if (requisito is null)
            {
                continue;
            }

            AdicionarDocumento(requisito, impedimentos);
        }

        foreach (var adicional in contexto.Requisitos
                     .Where(r => r.ObrigatorioAdicional())
                     .OrderBy(r => r.Codigo, StringComparer.Ordinal))
        {
            if (adicional.Situacao == SituacaoItemChecklist.Aprovado)
            {
                continue;
            }

            impedimentos.Add(Criar(
                CodigoImpedimentoLiberacao.RequisitoAdicionalPendente,
                adicional.Codigo,
                $"Requisito adicional {adicional.Codigo} pendente.",
                adicional.ItemChecklistId));
        }

        var podeTransicionar = contexto.SituacaoAtual is SituacaoMobilizacao.Aguardando or SituacaoMobilizacao.Liberado;
        var situacao = contexto.SituacaoAtual;
        if (podeTransicionar)
        {
            situacao = impedimentos.Count == 0
                ? SituacaoMobilizacao.Liberado
                : SituacaoMobilizacao.Aguardando;
        }

        var liberado = impedimentos.Count == 0 && situacao == SituacaoMobilizacao.Liberado;

        return new ResultadoLiberacao(
            contexto.MobilizacaoId,
            situacao,
            liberado,
            contexto.AvaliadoEmUtc,
            impedimentos);
    }

    private static void AdicionarIntegracao(ContextoLiberacaoTrabalhador contexto, List<ImpedimentoLiberacao> impedimentos)
    {
        var itemId = Requisito(contexto, CatalogoRequisitoCodigos.IntegracaoObra)?.ItemChecklistId;
        if (contexto.IntegracaoRefazer)
        {
            impedimentos.Add(Criar(
                CodigoImpedimentoLiberacao.IntegracaoRefazer,
                CatalogoRequisitoCodigos.IntegracaoObra,
                "Integração marcada para refazer.",
                itemId));
            return;
        }

        if (contexto.IntegracaoVencida)
        {
            impedimentos.Add(Criar(
                CodigoImpedimentoLiberacao.IntegracaoVencida,
                CatalogoRequisitoCodigos.IntegracaoObra,
                "Integração vencida.",
                itemId));
            return;
        }

        if (!contexto.IntegracaoValida)
        {
            impedimentos.Add(Criar(
                CodigoImpedimentoLiberacao.IntegracaoPendente,
                CatalogoRequisitoCodigos.IntegracaoObra,
                "Integração na obra pendente.",
                itemId));
        }
    }

    private static void AdicionarDocumento(RequisitoLiberacaoEntrada requisito, List<ImpedimentoLiberacao> impedimentos)
    {
        switch (requisito.Codigo)
        {
            case CatalogoRequisitoCodigos.DocOficialFoto:
                AdicionarPorSituacao(
                    requisito,
                    impedimentos,
                    CodigoImpedimentoLiberacao.IdentidadePendente,
                    null,
                    "Documento de identidade pendente.");
                break;
            case CatalogoRequisitoCodigos.EsocialVinculo:
                if (requisito.Situacao == SituacaoItemChecklist.Vencido)
                {
                    impedimentos.Add(Criar(
                        CodigoImpedimentoLiberacao.VinculoPreliminarVencido,
                        requisito.Codigo,
                        "Vínculo preliminar vencido.",
                        requisito.ItemChecklistId));
                    return;
                }

                if (requisito.Situacao is not SituacaoItemChecklist.Aprovado and not SituacaoItemChecklist.Preliminar)
                {
                    impedimentos.Add(Criar(
                        CodigoImpedimentoLiberacao.VinculoPendente,
                        requisito.Codigo,
                        "Vínculo eSocial pendente.",
                        requisito.ItemChecklistId));
                }

                break;
            case CatalogoRequisitoCodigos.AsoAdmissional:
                if (requisito.AsoInapto)
                {
                    impedimentos.Add(Criar(
                        CodigoImpedimentoLiberacao.AsoInapto,
                        requisito.Codigo,
                        "ASO registra trabalhador inapto.",
                        requisito.ItemChecklistId));
                    return;
                }

                AdicionarPorSituacao(
                    requisito,
                    impedimentos,
                    CodigoImpedimentoLiberacao.AsoPendente,
                    CodigoImpedimentoLiberacao.AsoVencido,
                    "ASO admissional pendente.",
                    "ASO admissional vencido.");
                break;
            case CatalogoRequisitoCodigos.Nr18Basica:
                AdicionarPorSituacao(
                    requisito,
                    impedimentos,
                    CodigoImpedimentoLiberacao.Nr18Pendente,
                    CodigoImpedimentoLiberacao.Nr18Vencida,
                    "Treinamento NR-18 pendente.",
                    "Treinamento NR-18 vencido.");
                break;
            case CatalogoRequisitoCodigos.OrdemServico:
                AdicionarPorSituacao(
                    requisito,
                    impedimentos,
                    CodigoImpedimentoLiberacao.OrdemServicoPendente,
                    null,
                    "Ordem de serviço pendente.");
                break;
        }
    }

    private static void AdicionarPorSituacao(
        RequisitoLiberacaoEntrada requisito,
        List<ImpedimentoLiberacao> impedimentos,
        string codigoPendente,
        string? codigoVencido,
        string motivoPendente,
        string? motivoVencido = null)
    {
        switch (requisito.Situacao)
        {
            case SituacaoItemChecklist.Aprovado:
            case SituacaoItemChecklist.Preliminar:
                return;
            case SituacaoItemChecklist.Vencido when codigoVencido is not null:
                impedimentos.Add(Criar(codigoVencido, requisito.Codigo, motivoVencido ?? motivoPendente, requisito.ItemChecklistId));
                return;
            default:
                impedimentos.Add(Criar(codigoPendente, requisito.Codigo, motivoPendente, requisito.ItemChecklistId));
                break;
        }
    }

    private static RequisitoLiberacaoEntrada? Requisito(ContextoLiberacaoTrabalhador contexto, string codigo) =>
        contexto.Requisitos.FirstOrDefault(r => string.Equals(r.Codigo, codigo, StringComparison.Ordinal));

    private static ImpedimentoLiberacao Criar(
        string codigo,
        string requisitoCodigo,
        string motivo,
        Guid? itemChecklistId) =>
        new(codigo, requisitoCodigo, motivo, itemChecklistId);

    private static bool ObrigatorioAdicional(this RequisitoLiberacaoEntrada entrada) =>
        !CodigosPadrao.Contains(entrada.Codigo);
}
