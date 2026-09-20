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

    [Fact]
    public void ObterSituacao_WithComprovanteBeforePrazo_IsNoPrazo()
    {
        var pagamento = CriarPagamento(new DateOnly(2026, 9, 10));
        pagamento.RegistrarComprovante("recibo.pdf", "key", "application/pdf", 1024);
        DefinirComprovanteEnviadoEm(pagamento, new DateTime(2026, 9, 12, 15, 0, 0, DateTimeKind.Utc));
        Assert.Equal(SituacaoComprovante.NoPrazo, pagamento.ObterSituacaoComprovante(new DateOnly(2026, 9, 13)));
    }

    [Fact]
    public void ObterSituacao_WithComprovanteAfterPrazo_IsEnviadoEmAtraso()
    {
        var pagamento = CriarPagamento(new DateOnly(2026, 9, 10));
        pagamento.RegistrarComprovante("recibo.pdf", "key", "application/pdf", 1024);
        DefinirComprovanteEnviadoEm(pagamento, new DateTime(2026, 9, 14, 10, 0, 0, DateTimeKind.Utc));
        Assert.Equal(
            SituacaoComprovante.EnviadoEmAtraso,
            pagamento.ObterSituacaoComprovante(new DateOnly(2026, 9, 20)));
    }

    private static void DefinirComprovanteEnviadoEm(PagamentoFuncionario pagamento, DateTime enviadoEm)
    {
        var property = typeof(PagamentoFuncionario).GetProperty(nameof(PagamentoFuncionario.ComprovanteEnviadoEm));
        Assert.NotNull(property);
        property.SetValue(pagamento, enviadoEm);
    }

    private static PagamentoFuncionario CriarPagamento(DateOnly dataPagamento) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(dataPagamento.Year, dataPagamento.Month, 1),
            dataPagamento);
}
