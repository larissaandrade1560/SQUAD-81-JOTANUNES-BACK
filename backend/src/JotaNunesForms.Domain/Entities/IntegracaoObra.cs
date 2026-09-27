namespace JotaNunesForms.Domain.Entities;

public sealed class IntegracaoObra
{
    public Guid Id { get; private set; }

    public Guid MobilizacaoId { get; private set; }

    public Guid ItemChecklistId { get; private set; }

    public Guid ObraId { get; private set; }

    public DateTimeOffset DataHora { get; private set; }

    public string Conteudo { get; private set; } = string.Empty;

    public string Instrutor { get; private set; } = string.Empty;

    public string? Avaliacao { get; private set; }

    public bool AceiteTrabalhador { get; private set; }

    public DateTime? ValidoAte { get; private set; }

    public bool Refazer { get; private set; }

    public DateTime? RefazerEm { get; private set; }

    public Guid? RefazerPorUsuarioId { get; private set; }

    public string? MotivoRefazer { get; private set; }

    public Guid CriadoPorUsuarioId { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public string PayloadHash { get; private set; } = string.Empty;

    public DateTime CriadoEm { get; private set; }

    private IntegracaoObra()
    {
    }

    public static IntegracaoObra Registrar(
        Guid mobilizacaoId,
        Guid itemChecklistId,
        Guid obraId,
        DateTimeOffset dataHora,
        string conteudo,
        string instrutor,
        string? avaliacao,
        bool aceiteTrabalhador,
        DateTime? validoAte,
        Guid criadoPorUsuarioId,
        string idempotencyKey,
        string payloadHash,
        DateTime utcNow,
        TimeSpan toleranciaFuturo)
    {
        ValidarChaves(mobilizacaoId, itemChecklistId, obraId, criadoPorUsuarioId, idempotencyKey, payloadHash);
        ValidarConteudo(conteudo, instrutor, avaliacao, aceiteTrabalhador);
        ValidarDataHora(dataHora, utcNow, toleranciaFuturo);
        if (validoAte is not null && validoAte.Value.ToUniversalTime() <= dataHora.UtcDateTime)
        {
            throw new ArgumentException("Validade deve ser posterior à data e hora da integração.", nameof(validoAte));
        }

        return new IntegracaoObra
        {
            Id = Guid.NewGuid(),
            MobilizacaoId = mobilizacaoId,
            ItemChecklistId = itemChecklistId,
            ObraId = obraId,
            DataHora = dataHora,
            Conteudo = conteudo.Trim(),
            Instrutor = instrutor.Trim(),
            Avaliacao = string.IsNullOrWhiteSpace(avaliacao) ? null : avaliacao.Trim(),
            AceiteTrabalhador = aceiteTrabalhador,
            ValidoAte = validoAte?.ToUniversalTime(),
            Refazer = false,
            CriadoPorUsuarioId = criadoPorUsuarioId,
            IdempotencyKey = idempotencyKey.Trim(),
            PayloadHash = payloadHash,
            CriadoEm = utcNow,
        };
    }

    public void MarcarParaRefazer(Guid usuarioId, string motivo, DateTime utcNow)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException("Usuário é obrigatório.", nameof(usuarioId));
        }

        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > 1000)
        {
            throw new ArgumentException("Motivo é obrigatório e deve ter até 1000 caracteres.", nameof(motivo));
        }

        Refazer = true;
        RefazerEm = utcNow;
        RefazerPorUsuarioId = usuarioId;
        MotivoRefazer = motivo.Trim();
    }

    public bool EstaVigenteEm(DateTime avaliadoEmUtc) =>
        AceiteTrabalhador
        && !Refazer
        && (ValidoAte is null || ValidoAte.Value.ToUniversalTime() > avaliadoEmUtc);

    private static void ValidarChaves(
        Guid mobilizacaoId,
        Guid itemChecklistId,
        Guid obraId,
        Guid criadoPorUsuarioId,
        string idempotencyKey,
        string payloadHash)
    {
        if (mobilizacaoId == Guid.Empty)
        {
            throw new ArgumentException("Mobilização é obrigatória.", nameof(mobilizacaoId));
        }

        if (itemChecklistId == Guid.Empty)
        {
            throw new ArgumentException("Item de checklist é obrigatório.", nameof(itemChecklistId));
        }

        if (obraId == Guid.Empty)
        {
            throw new ArgumentException("Obra é obrigatória.", nameof(obraId));
        }

        if (criadoPorUsuarioId == Guid.Empty)
        {
            throw new ArgumentException("Usuário criador é obrigatório.", nameof(criadoPorUsuarioId));
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Trim().Length > 100)
        {
            throw new ArgumentException("Chave de idempotência é obrigatória e deve ter até 100 caracteres.", nameof(idempotencyKey));
        }

        if (string.IsNullOrWhiteSpace(payloadHash) || payloadHash.Length != 64)
        {
            throw new ArgumentException("Hash do payload deve ter 64 caracteres.", nameof(payloadHash));
        }
    }

    private static void ValidarConteudo(string conteudo, string instrutor, string? avaliacao, bool aceiteTrabalhador)
    {
        if (string.IsNullOrWhiteSpace(conteudo) || conteudo.Trim().Length > 4000)
        {
            throw new ArgumentException("Conteúdo é obrigatório e deve ter até 4000 caracteres.", nameof(conteudo));
        }

        if (string.IsNullOrWhiteSpace(instrutor) || instrutor.Trim().Length > 200)
        {
            throw new ArgumentException("Instrutor é obrigatório e deve ter até 200 caracteres.", nameof(instrutor));
        }

        if (avaliacao?.Trim().Length > 1000)
        {
            throw new ArgumentException("Avaliação deve ter até 1000 caracteres.", nameof(avaliacao));
        }

        if (!aceiteTrabalhador)
        {
            throw new ArgumentException("Aceite do trabalhador é obrigatório para integração válida.", nameof(aceiteTrabalhador));
        }
    }

    private static void ValidarDataHora(DateTimeOffset dataHora, DateTime utcNow, TimeSpan toleranciaFuturo)
    {
        var limite = utcNow.Add(toleranciaFuturo);
        if (dataHora.UtcDateTime > limite)
        {
            throw new ArgumentException("Data e hora da integração não podem estar no futuro.", nameof(dataHora));
        }
    }
}
