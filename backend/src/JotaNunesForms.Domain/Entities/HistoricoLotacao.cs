namespace JotaNunesForms.Domain.Entities;

public sealed class HistoricoLotacao
{
    public Guid Id { get; private set; }

    public Guid MobilizacaoId { get; private set; }

    public Guid ObraId { get; private set; }

    public DateTime Inicio { get; private set; }

    public DateTime? Fim { get; private set; }

    public string? Motivo { get; private set; }

    private HistoricoLotacao()
    {
    }

    public HistoricoLotacao(Guid mobilizacaoId, Guid obraId, string? motivo = null)
    {
        if (mobilizacaoId == Guid.Empty)
        {
            throw new ArgumentException("Mobilização é obrigatória.", nameof(mobilizacaoId));
        }

        if (obraId == Guid.Empty)
        {
            throw new ArgumentException("Obra é obrigatória.", nameof(obraId));
        }

        Id = Guid.NewGuid();
        MobilizacaoId = mobilizacaoId;
        ObraId = obraId;
        Inicio = DateTime.UtcNow;
        Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
    }

    public void Encerrar(string? motivo = null)
    {
        Fim = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(motivo))
        {
            Motivo = motivo.Trim();
        }
    }
}
