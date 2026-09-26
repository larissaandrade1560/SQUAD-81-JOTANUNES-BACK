using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Domain.Tests;

public sealed class RecalcularSituacaoProcessoTests
{
    [Fact]
    public void Qualificado_WhenMandatoryCorporateAndSocioItemsApproved()
    {
        var processoId = Guid.NewGuid();
        var itens = new[]
        {
            Aprovado(processoId, TitularRequisito.Empresa),
            Aprovado(processoId, TitularRequisito.Empresa),
            Aprovado(processoId, TitularRequisito.Socio, ordem: 1),
            Aprovado(processoId, TitularRequisito.Contrato),
        };

        var situacao = RecalcularSituacaoProcesso.Calcular(SituacaoProcesso.Aberto, itens);

        Assert.Equal(SituacaoProcesso.Qualificado, situacao);
    }

    [Fact]
    public void RemainsAberto_WhenConditionalCorporatePending()
    {
        var processoId = Guid.NewGuid();
        var itens = new[]
        {
            Aprovado(processoId, TitularRequisito.Empresa),
            Item(processoId, TitularRequisito.Contrato, SituacaoItemChecklist.PendenteAnalise),
        };

        var situacao = RecalcularSituacaoProcesso.Calcular(SituacaoProcesso.Aberto, itens);

        Assert.Equal(SituacaoProcesso.Aberto, situacao);
    }

    [Fact]
    public void DoesNotConsiderWorkerItems_ForQualification()
    {
        var processoId = Guid.NewGuid();
        var itens = new[]
        {
            Aprovado(processoId, TitularRequisito.Empresa),
            Item(processoId, TitularRequisito.Trabalhador, SituacaoItemChecklist.NaoEnviado),
        };

        var situacao = RecalcularSituacaoProcesso.Calcular(SituacaoProcesso.Aberto, itens);

        Assert.Equal(SituacaoProcesso.Qualificado, situacao);
    }

    [Fact]
    public void Encerrado_IsPreserved()
    {
        var situacao = RecalcularSituacaoProcesso.Calcular(
            SituacaoProcesso.Encerrado,
            [Aprovado(Guid.NewGuid(), TitularRequisito.Empresa)]);

        Assert.Equal(SituacaoProcesso.Encerrado, situacao);
    }

    [Fact]
    public void AplicarVencimento_MarksApprovedItemExpired()
    {
        var item = Aprovado(Guid.NewGuid(), TitularRequisito.Empresa);
        RecalcularSituacaoProcesso.AplicarVencimento(
            item,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            permiteVencerComoDocumento: true);

        Assert.Equal(SituacaoItemChecklist.Vencido, item.Situacao);
    }

    [Fact]
    public void AplicarVencimento_SkipsWhenCatalogForbidsDocumentExpiry()
    {
        var item = Aprovado(Guid.NewGuid(), TitularRequisito.Empresa);
        RecalcularSituacaoProcesso.AplicarVencimento(
            item,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow,
            permiteVencerComoDocumento: false);

        Assert.Equal(SituacaoItemChecklist.Aprovado, item.Situacao);
    }

    private static ItemChecklist Aprovado(Guid processoId, TitularRequisito titular, int? ordem = null)
    {
        var item = Item(processoId, titular, SituacaoItemChecklist.Aprovado, ordem);
        return item;
    }

    private static ItemChecklist Item(
        Guid processoId,
        TitularRequisito titular,
        SituacaoItemChecklist situacao,
        int? ordem = null)
    {
        var item = new ItemChecklist(processoId, Guid.NewGuid(), titular, obrigatorio: true, titularOrdem: ordem);
        item.DefinirSituacao(situacao);
        return item;
    }
}
