using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Tests;

public sealed class FormularioTests
{
    [Fact]
    public void Constructor_WithValidTitle_CreatesFormulario()
    {
        var formulario = new Formulario("  Cadastro inicial  ", "  descrição  ");

        Assert.Equal("Cadastro inicial", formulario.Titulo);
        Assert.Equal("descrição", formulario.Descricao);
        Assert.True(formulario.CriadoEm <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException(string? titulo)
    {
        var exception = Assert.Throws<ArgumentException>(() => new Formulario(titulo!));

        Assert.Equal("titulo", exception.ParamName);
    }
}
