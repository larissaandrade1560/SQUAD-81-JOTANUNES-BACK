using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Domain.Tests;

public sealed class RegraLiberacaoTrabalhadorTests
{
    private static readonly DateTime AvaliadoEm = new(2026, 9, 27, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Liberado_WhenAllEightRequirementsAreSatisfied()
    {
        var mobilizacaoId = Guid.NewGuid();
        var contexto = Contexto(
            mobilizacaoId,
            cadastroCompleto: true,
            requisitos: TodosAprovados(mobilizacaoId),
            saldoEpi: 2,
            integracaoValida: true);

        var resultado = RegraLiberacaoTrabalhador.Avaliar(contexto);

        Assert.True(resultado.Liberado);
        Assert.Equal(SituacaoMobilizacao.Liberado, resultado.Situacao);
        Assert.Empty(resultado.Impedimentos);
    }

    [Theory]
    [InlineData(CatalogoRequisitoCodigos.DocOficialFoto, CodigoImpedimentoLiberacao.IdentidadePendente)]
    [InlineData(CatalogoRequisitoCodigos.EsocialVinculo, CodigoImpedimentoLiberacao.VinculoPendente)]
    [InlineData(CatalogoRequisitoCodigos.AsoAdmissional, CodigoImpedimentoLiberacao.AsoPendente)]
    [InlineData(CatalogoRequisitoCodigos.Nr18Basica, CodigoImpedimentoLiberacao.Nr18Pendente)]
    [InlineData(CatalogoRequisitoCodigos.OrdemServico, CodigoImpedimentoLiberacao.OrdemServicoPendente)]
    public void Pendente_ForEachMandatoryDocumentRequirement(string codigo, string impedimentoEsperado)
    {
        var mobilizacaoId = Guid.NewGuid();
        var requisitos = TodosAprovados(mobilizacaoId)
            .Select(r => r.Codigo == codigo
                ? r with { Situacao = SituacaoItemChecklist.NaoEnviado }
                : r)
            .ToList();

        var resultado = RegraLiberacaoTrabalhador.Avaliar(Contexto(
            mobilizacaoId,
            true,
            requisitos,
            saldoEpi: 1,
            integracaoValida: true));

        Assert.Contains(resultado.Impedimentos, i => i.Codigo == impedimentoEsperado);
        Assert.False(resultado.Liberado);
    }

    [Fact]
    public void VinculoPreliminarVencido_WhenEsocialItemIsExpired()
    {
        var mobilizacaoId = Guid.NewGuid();
        var requisitos = TodosAprovados(mobilizacaoId)
            .Select(r => r.Codigo == CatalogoRequisitoCodigos.EsocialVinculo
                ? r with { Situacao = SituacaoItemChecklist.Vencido }
                : r)
            .ToList();

        var resultado = RegraLiberacaoTrabalhador.Avaliar(Contexto(mobilizacaoId, true, requisitos, 1, true));

        Assert.Contains(resultado.Impedimentos, i => i.Codigo == CodigoImpedimentoLiberacao.VinculoPreliminarVencido);
    }

    [Fact]
    public void AsoInapto_BlocksReleaseEvenWhenOtherRequirementsAreValid()
    {
        var mobilizacaoId = Guid.NewGuid();
        var requisitos = TodosAprovados(mobilizacaoId)
            .Select(r => r.Codigo == CatalogoRequisitoCodigos.AsoAdmissional
                ? r with { Situacao = SituacaoItemChecklist.Rejeitado, AsoInapto = true }
                : r)
            .ToList();

        var resultado = RegraLiberacaoTrabalhador.Avaliar(Contexto(mobilizacaoId, true, requisitos, 1, true));

        Assert.Contains(resultado.Impedimentos, i => i.Codigo == CodigoImpedimentoLiberacao.AsoInapto);
    }

    [Fact]
    public void ProtectedState_IsNotPromotedToLiberado()
    {
        var mobilizacaoId = Guid.NewGuid();
        var contexto = new ContextoLiberacaoTrabalhador(
            mobilizacaoId,
            SituacaoMobilizacao.Afastado,
            true,
            TodosAprovados(mobilizacaoId),
            1,
            false,
            false,
            true,
            AvaliadoEm);

        var resultado = RegraLiberacaoTrabalhador.Avaliar(contexto);

        Assert.Equal(SituacaoMobilizacao.Afastado, resultado.Situacao);
        Assert.False(resultado.Liberado);
    }

    [Fact]
    public void Impediments_AreReturnedInStableOrder()
    {
        var mobilizacaoId = Guid.NewGuid();
        var requisitos = TodosAprovados(mobilizacaoId)
            .Select(r => r with { Situacao = SituacaoItemChecklist.NaoEnviado })
            .ToList();

        var resultado = RegraLiberacaoTrabalhador.Avaliar(Contexto(
            mobilizacaoId,
            cadastroCompleto: false,
            requisitos,
            saldoEpi: 0,
            integracaoValida: false));

        var codigos = resultado.Impedimentos.Select(i => i.Codigo).ToList();
        Assert.Equal(CodigoImpedimentoLiberacao.CadastroIncompleto, codigos[0]);
        Assert.True(codigos.IndexOf(CodigoImpedimentoLiberacao.IdentidadePendente)
            < codigos.IndexOf(CodigoImpedimentoLiberacao.EpiSemEntregaAtiva));
    }

    [Fact]
    public void RequisitoAdicionalPendente_ForUnknownMandatoryCatalogCode()
    {
        var mobilizacaoId = Guid.NewGuid();
        var requisitos = TodosAprovados(mobilizacaoId).Append(
            new RequisitoLiberacaoEntrada("EXTRA_OBRIGATORIO", Guid.NewGuid(), SituacaoItemChecklist.PendenteAnalise))
            .ToList();

        var resultado = RegraLiberacaoTrabalhador.Avaliar(Contexto(mobilizacaoId, true, requisitos, 1, true));

        Assert.Contains(resultado.Impedimentos, i => i.Codigo == CodigoImpedimentoLiberacao.RequisitoAdicionalPendente);
    }

    private static ContextoLiberacaoTrabalhador Contexto(
        Guid mobilizacaoId,
        bool cadastroCompleto,
        IReadOnlyList<RequisitoLiberacaoEntrada> requisitos,
        int saldoEpi,
        bool integracaoValida,
        SituacaoMobilizacao situacao = SituacaoMobilizacao.Aguardando) =>
        new(
            mobilizacaoId,
            situacao,
            cadastroCompleto,
            requisitos,
            saldoEpi,
            IntegracaoRefazer: false,
            IntegracaoVencida: false,
            IntegracaoValida: integracaoValida,
            AvaliadoEmUtc: AvaliadoEm);

    private static List<RequisitoLiberacaoEntrada> TodosAprovados(Guid mobilizacaoId) =>
    [
        new(CatalogoRequisitoCodigos.MobCadastro, Guid.NewGuid(), SituacaoItemChecklist.Aprovado),
        new(CatalogoRequisitoCodigos.DocOficialFoto, Guid.NewGuid(), SituacaoItemChecklist.Aprovado),
        new(CatalogoRequisitoCodigos.EsocialVinculo, Guid.NewGuid(), SituacaoItemChecklist.Aprovado),
        new(CatalogoRequisitoCodigos.AsoAdmissional, Guid.NewGuid(), SituacaoItemChecklist.Aprovado),
        new(CatalogoRequisitoCodigos.Nr18Basica, Guid.NewGuid(), SituacaoItemChecklist.Aprovado),
        new(CatalogoRequisitoCodigos.OrdemServico, Guid.NewGuid(), SituacaoItemChecklist.Aprovado),
        new(CatalogoRequisitoCodigos.EpiEntrega, Guid.NewGuid(), SituacaoItemChecklist.Aprovado),
        new(CatalogoRequisitoCodigos.IntegracaoObra, Guid.NewGuid(), SituacaoItemChecklist.Aprovado),
    ];
}
