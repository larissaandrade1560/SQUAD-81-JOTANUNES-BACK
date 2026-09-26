namespace JotaNunesForms.Domain.Entities;

public sealed class Mobilizacao
{
    public Guid Id { get; private set; }

    public Guid FuncionarioId { get; private set; }

    public Guid ContratoId { get; private set; }

    public Guid ObraId { get; private set; }

    public Guid ProcessoId { get; private set; }

    public string Funcao { get; private set; } = string.Empty;

    public DateOnly? DataFimObra { get; private set; }

    public string? TurnoJornada { get; private set; }

    public SituacaoMobilizacao Situacao { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private Mobilizacao()
    {
    }

    public Mobilizacao(
        Guid funcionarioId,
        Guid contratoId,
        Guid obraId,
        Guid processoId,
        string funcao,
        DateOnly? dataFimObra = null,
        string? turnoJornada = null)
    {
        if (funcionarioId == Guid.Empty)
        {
            throw new ArgumentException("Funcionário é obrigatório.", nameof(funcionarioId));
        }

        if (contratoId == Guid.Empty)
        {
            throw new ArgumentException("Contrato é obrigatório.", nameof(contratoId));
        }

        if (obraId == Guid.Empty)
        {
            throw new ArgumentException("Obra é obrigatória.", nameof(obraId));
        }

        if (processoId == Guid.Empty)
        {
            throw new ArgumentException("Processo é obrigatório.", nameof(processoId));
        }

        Id = Guid.NewGuid();
        FuncionarioId = funcionarioId;
        ContratoId = contratoId;
        ObraId = obraId;
        ProcessoId = processoId;
        Funcao = Normalize(funcao, "Função é obrigatória.");
        DataFimObra = dataFimObra;
        TurnoJornada = string.IsNullOrWhiteSpace(turnoJornada) ? null : turnoJornada.Trim();
        Situacao = SituacaoMobilizacao.Aguardando;
        CriadoEm = DateTime.UtcNow;
    }

    public void AtualizarCadastro(string funcao, DateOnly? dataFimObra, string? turnoJornada)
    {
        Funcao = Normalize(funcao, "Função é obrigatória.");
        DataFimObra = dataFimObra;
        TurnoJornada = string.IsNullOrWhiteSpace(turnoJornada) ? null : turnoJornada.Trim();
    }

    public void DefinirSituacao(SituacaoMobilizacao situacao) => Situacao = situacao;

    private static string Normalize(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message);
        }

        return value.Trim();
    }
}
