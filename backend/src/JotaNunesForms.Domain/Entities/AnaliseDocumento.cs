namespace JotaNunesForms.Domain.Entities;

public enum DecisaoAnalise
{
    Aprovado = 1,
    Rejeitado = 2,
}

public sealed class AnaliseDocumento
{
    public Guid Id { get; private set; }

    public Guid DocumentoVersaoId { get; private set; }

    public DecisaoAnalise Decisao { get; private set; }

    public string? Motivo { get; private set; }

    public string? Comentario { get; private set; }

    public Guid AnalistaUsuarioId { get; private set; }

    public DateTime AnalisadoEm { get; private set; }

    public DateTime? ValidoAte { get; private set; }

    private AnaliseDocumento()
    {
    }

    public AnaliseDocumento(
        Guid documentoVersaoId,
        DecisaoAnalise decisao,
        Guid analistaUsuarioId,
        string? motivo = null,
        string? comentario = null,
        DateTime? validoAte = null)
    {
        if (documentoVersaoId == Guid.Empty)
        {
            throw new ArgumentException("Versão é obrigatória.", nameof(documentoVersaoId));
        }

        if (analistaUsuarioId == Guid.Empty)
        {
            throw new ArgumentException("Analista é obrigatório.", nameof(analistaUsuarioId));
        }

        if (decisao == DecisaoAnalise.Rejeitado && string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("Motivo da rejeição é obrigatório.", nameof(motivo));
        }

        Id = Guid.NewGuid();
        DocumentoVersaoId = documentoVersaoId;
        Decisao = decisao;
        Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        Comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        AnalistaUsuarioId = analistaUsuarioId;
        AnalisadoEm = DateTime.UtcNow;
        ValidoAte = validoAte;
    }
}
