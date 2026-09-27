namespace JotaNunesForms.Domain.Entities;

public enum CodigoAuditoriaDocumento
{
    DocumentoEnviado = 1,
    DocumentoReenviado = 2,
    DocumentoAprovado = 3,
    DocumentoRejeitado = 4,
    DocumentoVencido = 5,
}

public enum TipoAtorAuditoria
{
    Usuario = 1,
    Sistema = 2,
}

public enum EscopoAuditoriaDocumento
{
    Empresa = 1,
    Funcionario = 2,
    Requisito = 3,
}

public enum OrigemDocumentoAuditoria
{
    DocumentoEmpresa = 1,
    DocumentoFuncionario = 2,
    DocumentoVersao = 3,
}

/// <summary>
/// Immutable, allowlisted record of a completed document transition.
/// </summary>
public sealed class EventoAuditoriaDocumento
{
    private const int MaxReasonLength = 2000;

    public Guid Id { get; private set; }
    public string ChaveNegocio { get; private set; } = string.Empty;
    public CodigoAuditoriaDocumento Codigo { get; private set; }
    public DateTime OcorreuEm { get; private set; }
    public TipoAtorAuditoria AtorTipo { get; private set; }
    public Guid? AtorUsuarioId { get; private set; }
    public string AtorNome { get; private set; } = string.Empty;
    public string? AtorPerfil { get; private set; }
    public bool Automatico => AtorTipo == TipoAtorAuditoria.Sistema;
    public EscopoAuditoriaDocumento Escopo { get; private set; }
    public OrigemDocumentoAuditoria OrigemTipo { get; private set; }
    public Guid DocumentoId { get; private set; }
    public Guid? VersaoId { get; private set; }
    public int? VersaoNumero { get; private set; }
    public Guid? VersaoAnteriorId { get; private set; }
    public int? VersaoAnteriorNumero { get; private set; }
    public Guid EmpresaId { get; private set; }
    public string EmpresaRazaoSocial { get; private set; } = string.Empty;
    public Guid? FuncionarioId { get; private set; }
    public string? FuncionarioNome { get; private set; }
    public Guid? ProcessoId { get; private set; }
    public Guid? ItemChecklistId { get; private set; }
    public string TipoDocumentoRotulo { get; private set; } = string.Empty;
    public string? Motivo { get; private set; }
    public string? Comentario { get; private set; }
    public DateTime? ValidoAte { get; private set; }

    private EventoAuditoriaDocumento()
    {
    }

    public static EventoAuditoriaDocumento Create(
        string chaveNegocio,
        CodigoAuditoriaDocumento codigo,
        DateTime ocorreuEm,
        TipoAtorAuditoria atorTipo,
        Guid? atorUsuarioId,
        string atorNome,
        string? atorPerfil,
        EscopoAuditoriaDocumento escopo,
        OrigemDocumentoAuditoria origemTipo,
        Guid documentoId,
        Guid? versaoId,
        int? versaoNumero,
        Guid? versaoAnteriorId,
        int? versaoAnteriorNumero,
        Guid empresaId,
        string empresaRazaoSocial,
        Guid? funcionarioId,
        string? funcionarioNome,
        Guid? processoId,
        Guid? itemChecklistId,
        string tipoDocumentoRotulo,
        string? motivo,
        string? comentario,
        DateTime? validoAte,
        Guid? id = null)
    {
        var normalizedKey = Required(chaveNegocio, nameof(chaveNegocio), 200);
        var normalizedActor = Required(atorNome, nameof(atorNome), 200);
        var normalizedCompany = Required(empresaRazaoSocial, nameof(empresaRazaoSocial), 200);
        var normalizedLabel = Required(tipoDocumentoRotulo, nameof(tipoDocumentoRotulo), 160);
        var normalizedProfile = Optional(atorPerfil, nameof(atorPerfil), 40);
        var normalizedEmployee = Optional(funcionarioNome, nameof(funcionarioNome), 200);
        var normalizedReason = Optional(motivo, nameof(motivo), MaxReasonLength);
        var normalizedComment = Optional(comentario, nameof(comentario), MaxReasonLength);

        if (!Enum.IsDefined(codigo) || !Enum.IsDefined(atorTipo) || !Enum.IsDefined(escopo) || !Enum.IsDefined(origemTipo))
        {
            throw new ArgumentException("Código, ator, escopo e origem devem ser válidos.");
        }

        if (id == Guid.Empty || documentoId == Guid.Empty || empresaId == Guid.Empty)
        {
            throw new ArgumentException("Identificadores da auditoria, documento e empresa devem ser válidos.");
        }

        if (atorTipo == TipoAtorAuditoria.Usuario)
        {
            if (atorUsuarioId is null || atorUsuarioId == Guid.Empty || normalizedProfile is null)
            {
                throw new ArgumentException("Ator humano exige usuário e perfil.", nameof(atorUsuarioId));
            }
        }
        else if (atorUsuarioId is not null || normalizedProfile is not null || normalizedActor != "Sistema")
        {
            throw new ArgumentException("Ator automático deve usar o snapshot Sistema sem usuário ou perfil.", nameof(atorTipo));
        }

        if (codigo == CodigoAuditoriaDocumento.DocumentoVencido && atorTipo != TipoAtorAuditoria.Sistema)
        {
            throw new ArgumentException("Vencimento automático deve ter ator Sistema.", nameof(atorTipo));
        }

        if (codigo != CodigoAuditoriaDocumento.DocumentoVencido && atorTipo != TipoAtorAuditoria.Usuario)
        {
            throw new ArgumentException("Ações documentais manuais exigem um usuário.", nameof(atorTipo));
        }

        if (versaoId is null || versaoId == Guid.Empty || versaoNumero is null or < 1)
        {
            throw new ArgumentException("O evento deve referenciar uma versão positiva.", nameof(versaoId));
        }

        if (codigo == CodigoAuditoriaDocumento.DocumentoReenviado)
        {
            if (versaoAnteriorId is null || versaoAnteriorId == Guid.Empty || versaoAnteriorNumero is null or < 1 || versaoAnteriorId == versaoId)
            {
                throw new ArgumentException("Reenvio exige referência à versão anterior, distinta da nova.", nameof(versaoAnteriorId));
            }
        }
        else if (versaoAnteriorId is not null || versaoAnteriorNumero is not null)
        {
            throw new ArgumentException("Somente um reenvio pode informar a versão substituída.");
        }

        if ((versaoAnteriorId is null) != (versaoAnteriorNumero is null))
        {
            throw new ArgumentException("Identificador e número da versão anterior devem ser informados juntos.");
        }

        if (escopo == EscopoAuditoriaDocumento.Funcionario)
        {
            if (funcionarioId is null || funcionarioId == Guid.Empty || normalizedEmployee is null)
            {
                throw new ArgumentException("Escopo de funcionário exige seu identificador e snapshot.", nameof(funcionarioId));
            }
        }
        else if (funcionarioId is not null || normalizedEmployee is not null)
        {
            throw new ArgumentException("Somente o escopo de funcionário aceita snapshot de funcionário.", nameof(funcionarioId));
        }

        if (origemTipo == OrigemDocumentoAuditoria.DocumentoEmpresa && escopo != EscopoAuditoriaDocumento.Empresa ||
            origemTipo == OrigemDocumentoAuditoria.DocumentoFuncionario && escopo != EscopoAuditoriaDocumento.Funcionario)
        {
            throw new ArgumentException("Origem documental e escopo não correspondem.", nameof(origemTipo));
        }

        if (origemTipo == OrigemDocumentoAuditoria.DocumentoVersao && (itemChecklistId is null || itemChecklistId == Guid.Empty))
        {
            throw new ArgumentException("Origem DocumentoVersao exige item de checklist.", nameof(itemChecklistId));
        }

        if (escopo == EscopoAuditoriaDocumento.Requisito && origemTipo != OrigemDocumentoAuditoria.DocumentoVersao)
        {
            throw new ArgumentException("Escopo de requisito exige uma versão de checklist.", nameof(origemTipo));
        }

        if (codigo == CodigoAuditoriaDocumento.DocumentoRejeitado)
        {
            if (normalizedReason is null)
            {
                throw new ArgumentException("Rejeição exige motivo.", nameof(motivo));
            }
        }
        else if (normalizedReason is not null)
        {
            throw new ArgumentException("Motivo só pode existir em evento de rejeição.", nameof(motivo));
        }

        if (codigo is not CodigoAuditoriaDocumento.DocumentoAprovado and not CodigoAuditoriaDocumento.DocumentoRejeitado && normalizedComment is not null)
        {
            throw new ArgumentException("Comentário só pode existir em aprovação ou rejeição.", nameof(comentario));
        }

        if (validoAte is not null && codigo is not CodigoAuditoriaDocumento.DocumentoAprovado and not CodigoAuditoriaDocumento.DocumentoVencido)
        {
            throw new ArgumentException("Validade só pode existir em aprovação ou vencimento.", nameof(validoAte));
        }

        RequireUtc(ocorreuEm, nameof(ocorreuEm));
        if (validoAte is not null)
        {
            RequireUtc(validoAte.Value, nameof(validoAte));
        }

        if (processoId == Guid.Empty || itemChecklistId == Guid.Empty)
        {
            throw new ArgumentException("Identificadores de contexto devem ser válidos.");
        }

        return new EventoAuditoriaDocumento
        {
            Id = id ?? Guid.NewGuid(),
            ChaveNegocio = normalizedKey,
            Codigo = codigo,
            OcorreuEm = ocorreuEm,
            AtorTipo = atorTipo,
            AtorUsuarioId = atorUsuarioId,
            AtorNome = normalizedActor,
            AtorPerfil = normalizedProfile,
            Escopo = escopo,
            OrigemTipo = origemTipo,
            DocumentoId = documentoId,
            VersaoId = versaoId,
            VersaoNumero = versaoNumero,
            VersaoAnteriorId = versaoAnteriorId,
            VersaoAnteriorNumero = versaoAnteriorNumero,
            EmpresaId = empresaId,
            EmpresaRazaoSocial = normalizedCompany,
            FuncionarioId = funcionarioId,
            FuncionarioNome = normalizedEmployee,
            ProcessoId = processoId,
            ItemChecklistId = itemChecklistId,
            TipoDocumentoRotulo = normalizedLabel,
            Motivo = normalizedReason,
            Comentario = normalizedComment,
            ValidoAte = validoAte,
        };
    }

    private static string Required(string value, string parameterName, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Valor obrigatório.", parameterName);
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"O valor excede o limite de {maxLength} caracteres.", parameterName);
        }

        return normalized;
    }

    private static string? Optional(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentException($"O valor excede o limite de {maxLength} caracteres.", parameterName);
        }

        return normalized;
    }

    private static void RequireUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Instantes devem ser informados em UTC.", parameterName);
        }
    }
}
