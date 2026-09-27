using JotaNunesForms.Application.Documentos.Validadores;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Tests;

public sealed class OrdemServicoValidadorTests
{
    [Fact]
    public void ExigeCienciaDoTrabalhador()
    {
        var validador = new OrdemServicoValidador();
        var ctx = CriarContexto();
        var json = """{"funcaoAtividades":"a","riscos":"b","medidasPreventivas":"c","proibicoes":"d","emergencia":"e","usoEpi":"f","data":"2026-01-01","cienciaTrabalhador":false}""";
        Assert.False(validador.Validar(json, ctx).Valido);
    }

    [Fact]
    public void AceitaCamposCompletos()
    {
        var validador = new OrdemServicoValidador();
        var ctx = CriarContexto();
        var json = """{"funcaoAtividades":"a","riscos":"b","medidasPreventivas":"c","proibicoes":"d","emergencia":"e","usoEpi":"f","data":"2026-01-01","cienciaTrabalhador":true}""";
        Assert.True(validador.Validar(json, ctx).Valido);
    }

    private static ValidacaoAdmissionalContexto CriarContexto()
    {
        var empresa = new Empresa("MO A", "11222333000181", TipoEmpresa.MaoDeObra);
        var funcionario = new Funcionario(empresa.Id, "João da Silva", "52998224725", "Pedreiro");
        var mobilizacao = new Mobilizacao(empresa.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Pedreiro");
        return new ValidacaoAdmissionalContexto(
            CatalogoRequisitoCodigos.OrdemServico,
            mobilizacao,
            funcionario,
            empresa,
            new Dictionary<string, string>(),
            DateTime.UtcNow);
    }
}
