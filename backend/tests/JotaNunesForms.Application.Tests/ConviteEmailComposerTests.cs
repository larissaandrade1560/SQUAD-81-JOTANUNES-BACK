using JotaNunesForms.Application.Convites;

namespace JotaNunesForms.Application.Tests;

public sealed class ConviteEmailComposerTests
{
    [Fact]
    public void Compose_IncludesRazaoSocialCtaAnd48Hours()
    {
        var (text, html) = ConviteEmailComposer.Compose(
            "http://localhost:5173",
            "Empresa Teste LTDA",
            "raw-token");

        Assert.Contains("Empresa Teste LTDA", text);
        Assert.Contains("48 horas", text);
        Assert.Contains("http://localhost:5173/definir-senha?token=raw-token", text);

        Assert.Contains("Empresa Teste LTDA", html);
        Assert.Contains("48 horas", html);
        Assert.Contains("http://localhost:5173/definir-senha?token=raw-token", html);
        Assert.Contains("Portal de Terceirizadas", html);
    }
}
