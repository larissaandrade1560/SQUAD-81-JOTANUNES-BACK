using JotaNunesForms.Application.Documentos.Validadores;
using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.Tests;

public sealed class DocumentoOficialValidadorTests
{
    private readonly DocumentoOficialValidador _validador = new();

    [Fact]
    public void AceitaDocumentoLegivelComDadosDoTrabalhador()
    {
        var contexto = CriarContexto();
        var json = """{"tipoDocumento":"RG","legivel":true,"nome":"João da Silva","cpf":"52998224725"}""";
        var resultado = _validador.Validar(json, contexto);
        Assert.True(resultado.Valido);
        Assert.NotNull(resultado.CamposJsonCanonico);
    }

    [Fact]
    public void RejeitaDocumentoIlegivel()
    {
        var contexto = CriarContexto();
        var json = """{"tipoDocumento":"RG","legivel":false,"nome":"João da Silva","cpf":"52998224725"}""";
        var resultado = _validador.Validar(json, contexto);
        Assert.False(resultado.Valido);
        Assert.Contains(resultado.Erros, e => e.Campo == "legivel");
    }

    [Fact]
    public void RejeitaCpfDivergente()
    {
        var contexto = CriarContexto();
        var json = """{"tipoDocumento":"CNH","legivel":true,"nome":"João da Silva","cpf":"39053344705"}""";
        var resultado = _validador.Validar(json, contexto);
        Assert.False(resultado.Valido);
        Assert.Contains(resultado.Erros, e => e.Campo == "cpf");
    }

    private static ValidacaoAdmissionalContexto CriarContexto()
    {
        var empresa = new Empresa("MO A", "11222333000181", TipoEmpresa.MaoDeObra);
        var funcionario = new Funcionario(empresa.Id, "João da Silva", "52998224725", "Pedreiro");
        var mobilizacao = new Mobilizacao(empresa.Id, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Pedreiro");
        return new ValidacaoAdmissionalContexto(
            CatalogoRequisitoCodigos.DocOficialFoto,
            mobilizacao,
            funcionario,
            empresa,
            new Dictionary<string, string>(),
            DateTime.UtcNow);
    }
}
