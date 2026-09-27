using JotaNunesForms.Application.Documentos.Validadores;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Tests;

public sealed class EsocialVinculoValidadorTests
{
    private readonly EsocialVinculoValidador _validador = new();

    [Fact]
    public void S2200_AprovaComCorrespondencia()
    {
        var (ctx, json) = CriarBase("S2200");
        var resultado = _validador.Validar(json, ctx);
        Assert.True(resultado.Valido);
        Assert.Equal(SituacaoItemChecklist.Aprovado, resultado.SituacaoDerivada);
    }

    [Fact]
    public void S2190_DerivarPreliminarEValidade()
    {
        var (ctx, json) = CriarBase("S2190");
        var resultado = _validador.Validar(json, ctx);
        Assert.True(resultado.Valido);
        Assert.Equal(SituacaoItemChecklist.Preliminar, resultado.SituacaoDerivada);
        Assert.NotNull(resultado.ValidoAte);
    }

    private static (ValidacaoAdmissionalContexto Ctx, string Json) CriarBase(string evento)
    {
        var empresa = new Empresa("MO A", "11222333000181", TipoEmpresa.MaoDeObra);
        var funcionario = new Funcionario(empresa.Id, "João da Silva", "52998224725", "Pedreiro");
        var mobilizacao = new Mobilizacao(empresa.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Pedreiro");
        var ctx = new ValidacaoAdmissionalContexto(
            CatalogoRequisitoCodigos.EsocialVinculo,
            mobilizacao,
            funcionario,
            empresa,
            new Dictionary<string, string> { [ParametroNormativoChaves.S2190PrazoSubstituicaoDias] = "30" },
            DateTime.UtcNow);
        var json = $$"""{"evento":"{{evento}}","cnpjEmpregador":"11222333000181","cpfTrabalhador":"52998224725","funcaoCategoria":"Pedreiro","dataAdmissao":"2026-01-01","situacaoVinculo":"Ativo"}""";
        return (ctx, json);
    }
}
