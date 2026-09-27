using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Tests;

public sealed class IntegracaoObraTests
{
    [Fact]
    public void RegistroValido_EstaVigente()
    {
        var integracao = IntegracaoObra.Registrar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(-1),
            "Conteúdo", "Instrutor", null, true, null, Guid.NewGuid(), "k", new string('a', 64),
            DateTime.UtcNow, TimeSpan.FromMinutes(5));
        Assert.True(integracao.EstaVigenteEm(DateTime.UtcNow));
    }

    [Fact]
    public void Refazer_InvalidaImediatamente()
    {
        var integracao = IntegracaoObra.Registrar(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(-1),
            "Conteúdo", "Instrutor", null, true, null, Guid.NewGuid(), "k", new string('a', 64),
            DateTime.UtcNow, TimeSpan.FromMinutes(5));
        integracao.MarcarParaRefazer(Guid.NewGuid(), "Repetir integração", DateTime.UtcNow);
        Assert.False(integracao.EstaVigenteEm(DateTime.UtcNow));
    }
}
