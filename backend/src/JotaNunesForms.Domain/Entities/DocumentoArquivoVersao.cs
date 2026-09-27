namespace JotaNunesForms.Domain.Entities;

/// <summary>
/// Preserves each uploaded object reference for legacy company/employee documents.
/// </summary>
public sealed class DocumentoArquivoVersao
{
    public Guid Id { get; private set; }
    public Guid? DocumentoEmpresaId { get; private set; }
    public Guid? DocumentoFuncionarioId { get; private set; }
    public int Numero { get; private set; }
    public string NomeArquivo { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long TamanhoBytes { get; private set; }
    public Guid? EnviadoPorUsuarioId { get; private set; }
    public DateTime EnviadoEm { get; private set; }
    public bool Vigente { get; private set; }

    private DocumentoArquivoVersao()
    {
    }

    public static DocumentoArquivoVersao ForEmpresa(
        Guid documentoEmpresaId,
        int numero,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes,
        Guid enviadoPorUsuarioId,
        DateTime enviadoEm,
        Guid? id = null) =>
        Create(documentoEmpresaId, null, numero, nomeArquivo, storageKey, contentType, tamanhoBytes,
            enviadoPorUsuarioId, enviadoEm, id);

    public static DocumentoArquivoVersao ForFuncionario(
        Guid documentoFuncionarioId,
        int numero,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes,
        Guid enviadoPorUsuarioId,
        DateTime enviadoEm,
        Guid? id = null) =>
        Create(null, documentoFuncionarioId, numero, nomeArquivo, storageKey, contentType, tamanhoBytes,
            enviadoPorUsuarioId, enviadoEm, id);

    public static DocumentoArquivoVersao BaselineEmpresa(
        Guid documentoEmpresaId,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes,
        DateTime enviadoEm,
        Guid? id = null) =>
        Create(documentoEmpresaId, null, 1, nomeArquivo, storageKey, contentType, tamanhoBytes,
            null, enviadoEm, id);

    public static DocumentoArquivoVersao BaselineFuncionario(
        Guid documentoFuncionarioId,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes,
        DateTime enviadoEm,
        Guid? id = null) =>
        Create(null, documentoFuncionarioId, 1, nomeArquivo, storageKey, contentType, tamanhoBytes,
            null, enviadoEm, id);

    public void MarkNotCurrent() => Vigente = false;

    private static DocumentoArquivoVersao Create(
        Guid? documentoEmpresaId,
        Guid? documentoFuncionarioId,
        int numero,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes,
        Guid? enviadoPorUsuarioId,
        DateTime enviadoEm,
        Guid? id)
    {
        if ((documentoEmpresaId is null) == (documentoFuncionarioId is null) ||
            documentoEmpresaId == Guid.Empty || documentoFuncionarioId == Guid.Empty)
        {
            throw new ArgumentException("Informe exatamente uma origem documental válida.");
        }

        if (id == Guid.Empty || numero < 1 || enviadoPorUsuarioId == Guid.Empty || tamanhoBytes <= 0)
        {
            throw new ArgumentException("Id, número, remetente e tamanho devem ser válidos.");
        }

        var normalizedName = Required(nomeArquivo, nameof(nomeArquivo), 260);
        var normalizedKey = Required(storageKey, nameof(storageKey), 512);
        var normalizedContentType = Required(contentType, nameof(contentType), 128);
        if (enviadoEm.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Instante do envio deve ser informado em UTC.", nameof(enviadoEm));
        }

        return new DocumentoArquivoVersao
        {
            Id = id ?? Guid.NewGuid(),
            DocumentoEmpresaId = documentoEmpresaId,
            DocumentoFuncionarioId = documentoFuncionarioId,
            Numero = numero,
            NomeArquivo = normalizedName,
            StorageKey = normalizedKey,
            ContentType = normalizedContentType,
            TamanhoBytes = tamanhoBytes,
            EnviadoPorUsuarioId = enviadoPorUsuarioId,
            EnviadoEm = enviadoEm,
            Vigente = true,
        };
    }

    private static string Required(string? value, string parameterName, int maxLength)
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
}
