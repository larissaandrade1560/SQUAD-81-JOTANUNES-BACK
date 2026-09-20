namespace JotaNunesForms.Domain.Entities;

public sealed class PagamentoFuncionario
{
    public Guid Id { get; private set; }

    public Guid FuncionarioId { get; private set; }

    public Guid EmpresaId { get; private set; }

    /// <summary>Primeiro dia do mês de competência (folha).</summary>
    public DateOnly Competencia { get; private set; }

    public DateOnly DataPagamento { get; private set; }

    /// <summary>RF15 — data do pagamento + 3 dias corridos.</summary>
    public DateOnly PrazoComprovante { get; private set; }

    public DateTime? ComprovanteEnviadoEm { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private PagamentoFuncionario()
    {
    }

    public PagamentoFuncionario(
        Guid funcionarioId,
        Guid empresaId,
        DateOnly competencia,
        DateOnly dataPagamento)
    {
        if (funcionarioId == Guid.Empty)
        {
            throw new ArgumentException("Funcionário é obrigatório.", nameof(funcionarioId));
        }

        if (empresaId == Guid.Empty)
        {
            throw new ArgumentException("Empresa é obrigatória.", nameof(empresaId));
        }

        if (competencia.Day != 1)
        {
            throw new ArgumentException("Competência deve ser o primeiro dia do mês.", nameof(competencia));
        }

        Id = Guid.NewGuid();
        FuncionarioId = funcionarioId;
        EmpresaId = empresaId;
        Competencia = competencia;
        DataPagamento = dataPagamento;
        PrazoComprovante = CalcularPrazoComprovante(dataPagamento);
        CriadoEm = DateTime.UtcNow;
    }

    public static DateOnly CalcularPrazoComprovante(DateOnly dataPagamento) =>
        dataPagamento.AddDays(3);

    public SituacaoComprovante ObterSituacaoComprovante(DateOnly referenciaUtc)
    {
        if (ComprovanteEnviadoEm is null)
        {
            return referenciaUtc > PrazoComprovante
                ? SituacaoComprovante.EmAtraso
                : SituacaoComprovante.Pendente;
        }

        var enviado = DateOnly.FromDateTime(ComprovanteEnviadoEm.Value.ToUniversalTime());
        return enviado <= PrazoComprovante
            ? SituacaoComprovante.NoPrazo
            : SituacaoComprovante.EnviadoEmAtraso;
    }
}
