using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Services;

namespace JotaNunesForms.Domain.Tests;

public sealed class MovimentoEpiTests
{
    [Fact]
    public void EntregaAumentaSaldo()
    {
        var movimento = MovimentoEpi.RegistrarEntrega(
            Guid.NewGuid(), Guid.NewGuid(), "Luva", 2, "CA1", DateOnly.FromDateTime(DateTime.UtcNow),
            true, true, true, Guid.NewGuid(), "k1", new string('a', 64), DateOnly.FromDateTime(DateTime.UtcNow));
        Assert.Equal(2, SaldoEpi.CalcularSaldoAtivo([movimento]));
    }

    [Fact]
    public void DevolucaoParcialReduzSaldo()
    {
        var mob = Guid.NewGuid();
        var item = Guid.NewGuid();
        var entrega = MovimentoEpi.RegistrarEntrega(
            mob, item, "Luva", 2, "CA1", DateOnly.FromDateTime(DateTime.UtcNow),
            true, true, true, Guid.NewGuid(), "k1", new string('a', 64), DateOnly.FromDateTime(DateTime.UtcNow));
        var devolucao = MovimentoEpi.RegistrarDevolucao(
            mob, item, entrega.Id, "Luva", 1, "CA1", DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid(), "k2", new string('b', 64), DateOnly.FromDateTime(DateTime.UtcNow));
        Assert.Equal(1, SaldoEpi.CalcularSaldoAtivo([entrega, devolucao]));
    }

    [Fact]
    public void DevolucaoAcimaDoSaldo_Falha()
    {
        var mob = Guid.NewGuid();
        var item = Guid.NewGuid();
        var entrega = MovimentoEpi.RegistrarEntrega(
            mob, item, "Luva", 1, "CA1", DateOnly.FromDateTime(DateTime.UtcNow),
            true, true, true, Guid.NewGuid(), "k1", new string('a', 64), DateOnly.FromDateTime(DateTime.UtcNow));
        var devolucao = MovimentoEpi.RegistrarDevolucao(
            mob, item, entrega.Id, "Luva", 2, "CA1", DateOnly.FromDateTime(DateTime.UtcNow),
            Guid.NewGuid(), "k2", new string('b', 64), DateOnly.FromDateTime(DateTime.UtcNow));
        Assert.Throws<InvalidOperationException>(() => SaldoEpi.CalcularSaldoAtivo([entrega, devolucao]));
    }
}
