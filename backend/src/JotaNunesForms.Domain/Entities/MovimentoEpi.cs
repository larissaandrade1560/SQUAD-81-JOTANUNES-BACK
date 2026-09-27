namespace JotaNunesForms.Domain.Entities;

public sealed class MovimentoEpi
{
    public Guid Id { get; private set; }

    public Guid MobilizacaoId { get; private set; }

    public Guid ItemChecklistId { get; private set; }

    public TipoMovimentoEpi Tipo { get; private set; }

    public Guid? MovimentoOrigemId { get; private set; }

    public string Epi { get; private set; } = string.Empty;

    public int Quantidade { get; private set; }

    public string NumeroCa { get; private set; } = string.Empty;

    public DateOnly Data { get; private set; }

    public bool OrientacaoUso { get; private set; }

    public bool ResponsabilidadeGuarda { get; private set; }

    public bool AceiteTrabalhador { get; private set; }

    public Guid RegistradoPorUsuarioId { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public string PayloadHash { get; private set; } = string.Empty;

    public DateTime CriadoEm { get; private set; }

    private MovimentoEpi()
    {
    }

    public static MovimentoEpi RegistrarEntrega(
        Guid mobilizacaoId,
        Guid itemChecklistId,
        string epi,
        int quantidade,
        string numeroCa,
        DateOnly data,
        bool orientacaoUso,
        bool responsabilidadeGuarda,
        bool aceiteTrabalhador,
        Guid registradoPorUsuarioId,
        string idempotencyKey,
        string payloadHash,
        DateOnly operacaoEm)
    {
        ValidarChaves(mobilizacaoId, itemChecklistId, registradoPorUsuarioId, idempotencyKey, payloadHash);
        ValidarEntrega(epi, quantidade, numeroCa, data, orientacaoUso, responsabilidadeGuarda, aceiteTrabalhador, operacaoEm);

        return new MovimentoEpi
        {
            Id = Guid.NewGuid(),
            MobilizacaoId = mobilizacaoId,
            ItemChecklistId = itemChecklistId,
            Tipo = TipoMovimentoEpi.Entrega,
            Epi = epi.Trim(),
            Quantidade = quantidade,
            NumeroCa = numeroCa.Trim(),
            Data = data,
            OrientacaoUso = orientacaoUso,
            ResponsabilidadeGuarda = responsabilidadeGuarda,
            AceiteTrabalhador = aceiteTrabalhador,
            RegistradoPorUsuarioId = registradoPorUsuarioId,
            IdempotencyKey = idempotencyKey.Trim(),
            PayloadHash = payloadHash,
            CriadoEm = DateTime.UtcNow,
        };
    }

    public static MovimentoEpi RegistrarSubstituicao(
        Guid mobilizacaoId,
        Guid itemChecklistId,
        Guid movimentoOrigemId,
        string epi,
        int quantidade,
        string numeroCa,
        DateOnly data,
        bool orientacaoUso,
        bool responsabilidadeGuarda,
        bool aceiteTrabalhador,
        Guid registradoPorUsuarioId,
        string idempotencyKey,
        string payloadHash,
        DateOnly operacaoEm)
    {
        ValidarChaves(mobilizacaoId, itemChecklistId, registradoPorUsuarioId, idempotencyKey, payloadHash);
        if (movimentoOrigemId == Guid.Empty)
        {
            throw new ArgumentException("Movimento de origem é obrigatório para substituição.", nameof(movimentoOrigemId));
        }

        ValidarEntrega(epi, quantidade, numeroCa, data, orientacaoUso, responsabilidadeGuarda, aceiteTrabalhador, operacaoEm);

        return new MovimentoEpi
        {
            Id = Guid.NewGuid(),
            MobilizacaoId = mobilizacaoId,
            ItemChecklistId = itemChecklistId,
            Tipo = TipoMovimentoEpi.Substituicao,
            MovimentoOrigemId = movimentoOrigemId,
            Epi = epi.Trim(),
            Quantidade = quantidade,
            NumeroCa = numeroCa.Trim(),
            Data = data,
            OrientacaoUso = orientacaoUso,
            ResponsabilidadeGuarda = responsabilidadeGuarda,
            AceiteTrabalhador = aceiteTrabalhador,
            RegistradoPorUsuarioId = registradoPorUsuarioId,
            IdempotencyKey = idempotencyKey.Trim(),
            PayloadHash = payloadHash,
            CriadoEm = DateTime.UtcNow,
        };
    }

    public static MovimentoEpi RegistrarDevolucao(
        Guid mobilizacaoId,
        Guid itemChecklistId,
        Guid movimentoOrigemId,
        string epi,
        int quantidade,
        string numeroCa,
        DateOnly data,
        Guid registradoPorUsuarioId,
        string idempotencyKey,
        string payloadHash,
        DateOnly operacaoEm)
    {
        ValidarChaves(mobilizacaoId, itemChecklistId, registradoPorUsuarioId, idempotencyKey, payloadHash);
        if (movimentoOrigemId == Guid.Empty)
        {
            throw new ArgumentException("Movimento de origem é obrigatório para devolução.", nameof(movimentoOrigemId));
        }

        if (string.IsNullOrWhiteSpace(epi))
        {
            throw new ArgumentException("EPI é obrigatório.", nameof(epi));
        }

        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        }

        if (string.IsNullOrWhiteSpace(numeroCa))
        {
            throw new ArgumentException("Número do CA é obrigatório.", nameof(numeroCa));
        }

        if (data > operacaoEm)
        {
            throw new ArgumentException("Data do movimento não pode ser posterior à data da operação.", nameof(data));
        }

        return new MovimentoEpi
        {
            Id = Guid.NewGuid(),
            MobilizacaoId = mobilizacaoId,
            ItemChecklistId = itemChecklistId,
            Tipo = TipoMovimentoEpi.Devolucao,
            MovimentoOrigemId = movimentoOrigemId,
            Epi = epi.Trim(),
            Quantidade = quantidade,
            NumeroCa = numeroCa.Trim(),
            Data = data,
            OrientacaoUso = false,
            ResponsabilidadeGuarda = false,
            AceiteTrabalhador = false,
            RegistradoPorUsuarioId = registradoPorUsuarioId,
            IdempotencyKey = idempotencyKey.Trim(),
            PayloadHash = payloadHash,
            CriadoEm = DateTime.UtcNow,
        };
    }

    private static void ValidarChaves(
        Guid mobilizacaoId,
        Guid itemChecklistId,
        Guid registradoPorUsuarioId,
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

        if (registradoPorUsuarioId == Guid.Empty)
        {
            throw new ArgumentException("Usuário registrador é obrigatório.", nameof(registradoPorUsuarioId));
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

    private static void ValidarEntrega(
        string epi,
        int quantidade,
        string numeroCa,
        DateOnly data,
        bool orientacaoUso,
        bool responsabilidadeGuarda,
        bool aceiteTrabalhador,
        DateOnly operacaoEm)
    {
        if (string.IsNullOrWhiteSpace(epi) || epi.Trim().Length > 200)
        {
            throw new ArgumentException("EPI é obrigatório e deve ter até 200 caracteres.", nameof(epi));
        }

        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.", nameof(quantidade));
        }

        if (string.IsNullOrWhiteSpace(numeroCa) || numeroCa.Trim().Length > 50)
        {
            throw new ArgumentException("Número do CA é obrigatório e deve ter até 50 caracteres.", nameof(numeroCa));
        }

        if (data > operacaoEm)
        {
            throw new ArgumentException("Data do movimento não pode ser posterior à data da operação.", nameof(data));
        }

        if (!orientacaoUso || !responsabilidadeGuarda || !aceiteTrabalhador)
        {
            throw new ArgumentException("Entrega válida exige orientação, responsabilidade de guarda e aceite do trabalhador.");
        }
    }
}
