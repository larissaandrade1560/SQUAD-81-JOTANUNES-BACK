using JotaNunesForms.Application.Documentos.Validadores;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Tests;

public sealed class AsoValidadorTests
{
    private readonly AsoValidador _validador = new();

    [Fact]
    public void Apto_DerivaValidadePorParametro()
    {
        var ctx = CriarContexto();
        var json = BaseJson("Apto");
        var resultado = _validador.Validar(json, ctx);
        Assert.True(resultado.Valido);
        Assert.NotNull(resultado.ValidoAte);
    }

    [Fact]
    public void Inapto_Falha()
    {
        var ctx = CriarContexto();
        var resultado = _validador.Validar(BaseJson("Inapto"), ctx);
        Assert.False(resultado.Valido);
    }

    private static ValidacaoAdmissionalContexto CriarContexto()
    {
        var empresa = new Empresa("MO A", "11222333000181", TipoEmpresa.MaoDeObra);
        var funcionario = new Funcionario(empresa.Id, "João da Silva", "52998224725", "Pedreiro");
        var mobilizacao = new Mobilizacao(empresa.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Pedreiro");
        return new ValidacaoAdmissionalContexto(
            CatalogoRequisitoCodigos.AsoAdmissional,
            mobilizacao,
            funcionario,
            empresa,
            new Dictionary<string, string> { [ParametroNormativoChaves.AsoPeriodicidadeDias] = "365" },
            DateTime.UtcNow);
    }

    private static string BaseJson(string conclusao) => $$"""
    {"trabalhador":"João da Silva","empresa":"MO A","funcao":"Pedreiro","riscos":["Queda"],"atividadesEspecificas":["Alvenaria"],"dataExame":"2026-01-10","medico":"Dr. A","crm":"12345","conclusao":"{{conclusao}}","validacaoAssinatura":"ok"}
    """;
}
