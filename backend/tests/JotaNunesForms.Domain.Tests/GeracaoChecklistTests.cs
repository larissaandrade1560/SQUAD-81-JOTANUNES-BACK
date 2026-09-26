using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Domain.Tests;

public sealed class GeracaoChecklistTests
{
    private static readonly IReadOnlyList<CatalogoRequisito> Catalogo = CatalogoMvpFactory.CriarRequisitos();

    [Fact]
    public void ProcessoBasico_IncludesFiveCorporateDocs_AndOneSocioPair()
    {
        var processo = CriarProcesso(socios: 1);
        var itens = GeracaoChecklist.Gerar(processo, TipoEmpresa.MaoDeObra, Catalogo);

        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.ContratoSocial);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.ComprovanteCnpj);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.EnderecoComercial);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.CrfFgts);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.CndFederal);
        Assert.Equal(2, itens.Count(i => i.Titular == TitularRequisito.Socio));
        Assert.DoesNotContain(itens, i => i.Codigo == CatalogoRequisitoCodigos.LicencaMunicipalResiduos);
        Assert.DoesNotContain(itens, i => i.Codigo == CatalogoRequisitoCodigos.OpcaoSimples);
        Assert.DoesNotContain(itens, i => i.Codigo == CatalogoRequisitoCodigos.CgcreInmetro);
        Assert.DoesNotContain(itens, i => i.Codigo == CatalogoRequisitoCodigos.Art);
        Assert.DoesNotContain(itens, i => i.Titular == TitularRequisito.Trabalhador);
    }

    [Fact]
    public void ProcessoCondicional_IncludesResiduosSimplesLabArt_AndFourSocioItems()
    {
        var processo = CriarProcesso(
            socios: 2,
            residuos: true,
            laboratorio: true,
            simples: true,
            art: true,
            mobiliza: true);
        var itens = GeracaoChecklist.Gerar(processo, TipoEmpresa.MaoDeObra, Catalogo);

        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.LicencaMunicipalResiduos);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.CgcreInmetro);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.OpcaoSimples);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.Art);
        Assert.Equal(4, itens.Count(i => i.Titular == TitularRequisito.Socio));
        Assert.DoesNotContain(itens, i => i.Titular == TitularRequisito.Trabalhador);
    }

    [Fact]
    public void MateriaisSemMobilizacao_HasNoWorkerItems()
    {
        var processo = CriarProcesso(socios: 1, mobiliza: false);
        var itens = GeracaoChecklist.Gerar(processo, TipoEmpresa.Materiais, Catalogo);
        Assert.DoesNotContain(itens, i => i.Titular == TitularRequisito.Trabalhador);
    }

    [Fact]
    public void MateriaisComMobilizacao_Throws()
    {
        var processo = CriarProcesso(socios: 0, mobiliza: true);
        Assert.Throws<InvalidOperationException>(() =>
            GeracaoChecklist.Gerar(processo, TipoEmpresa.Materiais, Catalogo));
    }

    [Fact]
    public void GerarAdmissional_ReturnsMvpWorkerSet()
    {
        var itens = GeracaoChecklist.GerarAdmissional(Catalogo);
        Assert.Equal(8, itens.Count);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.IntegracaoObra);
        Assert.Contains(itens, i => i.Codigo == CatalogoRequisitoCodigos.EpiEntrega);
    }

    private static ProcessoContratacao CriarProcesso(
        int socios,
        bool residuos = false,
        bool laboratorio = false,
        bool simples = false,
        bool art = false,
        bool mobiliza = false) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Serviço de teste",
            residuos,
            laboratorio,
            simples,
            art,
            socios,
            mobiliza);
}
