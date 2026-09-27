using JotaNunesForms.Application.Documentos.Validadores;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Tests;

public sealed class Nr18ValidadorTests
{
    [Fact]
    public void RejeitaCargaHorariaAbaixoDoCatalogo()
    {
        var validador = new Nr18Validador();
        var ctx = CriarContexto();
        var json = """{"trabalhador":"João da Silva","treinamentoInicial":true,"conteudo":"NR18","cargaHorariaHoras":2,"data":"2026-01-01","local":"Obra","instrutor":"A","responsavelTecnico":"B","assinatura":"ok"}""";
        Assert.False(validador.Validar(json, ctx).Valido);
    }

    [Fact]
    public void AceitaCargaHorariaMinima()
    {
        var validador = new Nr18Validador();
        var ctx = CriarContexto();
        var json = """{"trabalhador":"João da Silva","treinamentoInicial":true,"conteudo":"NR18","cargaHorariaHoras":4,"data":"2026-01-01","local":"Obra","instrutor":"A","responsavelTecnico":"B","assinatura":"ok"}""";
        var resultado = validador.Validar(json, ctx);
        Assert.True(resultado.Valido);
        Assert.NotNull(resultado.ValidoAte);
    }

    private static ValidacaoAdmissionalContexto CriarContexto()
    {
        var empresa = new Empresa("MO A", "11222333000181", TipoEmpresa.MaoDeObra);
        var funcionario = new Funcionario(empresa.Id, "João da Silva", "52998224725", "Pedreiro");
        var mobilizacao = new Mobilizacao(empresa.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Pedreiro");
        return new ValidacaoAdmissionalContexto(
            CatalogoRequisitoCodigos.Nr18Basica,
            mobilizacao,
            funcionario,
            empresa,
            new Dictionary<string, string>
            {
                [ParametroNormativoChaves.Nr18CargaHorariaInicial] = "4",
                [ParametroNormativoChaves.Nr18PeriodicidadeMeses] = "24",
            },
            DateTime.UtcNow);
    }
}
