using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Tests;

public sealed class ConviteAcessoTests
{
    [Fact]
    public void PodeUsar_ReturnsTrue_WhenWithin48HoursAndNotUsed()
    {
        var criado = new DateTime(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc);
        var convite = new ConviteAcesso(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a@b.com",
            "hash",
            criado.AddHours(48),
            Guid.NewGuid(),
            criado);

        Assert.True(convite.PodeUsar(criado.AddHours(47)));
    }

    [Fact]
    public void PodeUsar_ReturnsFalse_WhenExpired()
    {
        var criado = new DateTime(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc);
        var convite = new ConviteAcesso(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a@b.com",
            "hash",
            criado.AddHours(48),
            Guid.NewGuid(),
            criado);

        Assert.False(convite.PodeUsar(criado.AddHours(49)));
    }

    [Fact]
    public void PodeUsar_ReturnsFalse_WhenUsed()
    {
        var criado = new DateTime(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc);
        var convite = new ConviteAcesso(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a@b.com",
            "hash",
            criado.AddHours(48),
            Guid.NewGuid(),
            criado);
        convite.MarcarUsado(criado.AddHours(1));

        Assert.False(convite.PodeUsar(criado.AddHours(2)));
    }

    [Fact]
    public void PodeUsar_ReturnsFalse_WhenInvalidated()
    {
        var criado = new DateTime(2026, 9, 26, 12, 0, 0, DateTimeKind.Utc);
        var convite = new ConviteAcesso(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "a@b.com",
            "hash",
            criado.AddHours(48),
            Guid.NewGuid(),
            criado);
        convite.Invalidar(criado.AddHours(1));

        Assert.False(convite.PodeUsar(criado.AddHours(2)));
    }
}
