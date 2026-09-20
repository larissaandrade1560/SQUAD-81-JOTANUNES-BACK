using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Tests;

public sealed class PagamentoFuncionarioTests
{
    [Fact]
    public void CalcularPrazoComprovante_AddsThreeCalendarDays()
    {
        var prazo = PagamentoFuncionario.CalcularPrazoComprovante(new DateOnly(2026, 9, 10));
        Assert.Equal(new DateOnly(2026, 9, 13), prazo);
    }

    [Fact]
    public void ObterSituacao_WithoutComprovanteBeforePrazo_IsPendente()
    {
        var pagamento = CriarPagamento(new DateOnly(2026, 9, 10));
        Assert.Equal(SituacaoComprovante.Pendente, pagamento.ObterSituacaoComprovante(new DateOnly(2026, 9, 12)));
    }

    [Fact]
    public void ObterSituacao_WithoutComprovanteAfterPrazo_IsEmAtraso()
    {
        var pagamento = CriarPagamento(new DateOnly(2026, 9, 10));
        Assert.Equal(SituacaoComprovante.EmAtraso, pagamento.ObterSituacaoComprovante(new DateOnly(2026, 9, 14)));
    }

    private static PagamentoFuncionario CriarPagamento(DateOnly dataPagamento) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(dataPagamento.Year, dataPagamento.Month, 1),
            dataPagamento);
}
