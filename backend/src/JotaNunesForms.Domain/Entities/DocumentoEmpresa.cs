namespace JotaNunesForms.Domain.Entities;

public sealed class DocumentoEmpresa
{
    public Guid Id { get; private set; }

    public Guid EmpresaId { get; private set; }

    public TipoDocumentoEmpresarial Tipo { get; private set; }

    public string NomeArquivo { get; private set; } = string.Empty;

    public string StorageKey { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long TamanhoBytes { get; private set; }

    public StatusDocumento Status { get; private set; }

    public DateTime EnviadoEm { get; private set; }

    private DocumentoEmpresa()
    {
    }

    public DocumentoEmpresa(
        Guid empresaId,
        TipoDocumentoEmpresarial tipo,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes)
        : this(Guid.NewGuid(), empresaId, tipo, nomeArquivo, storageKey, contentType, tamanhoBytes)
    {
    }

    public DocumentoEmpresa(
        Guid id,
        Guid empresaId,
        TipoDocumentoEmpresarial tipo,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ArgumentException("Empresa é obrigatória.", nameof(empresaId));
        }

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id é obrigatório.", nameof(id));
        }

        Id = id;
        EmpresaId = empresaId;
        Tipo = tipo;
        NomeArquivo = NormalizeNomeArquivo(nomeArquivo);
        StorageKey = NormalizeStorageKey(storageKey);
        ContentType = contentType;
        TamanhoBytes = tamanhoBytes;
        Status = StatusDocumento.Pendente;
        EnviadoEm = DateTime.UtcNow;
    }

    private static string NormalizeNomeArquivo(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome do arquivo é obrigatório.", nameof(nome));
        }

        return nome.Trim();
    }

    private static string NormalizeStorageKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Chave de storage é obrigatória.", nameof(key));
        }

        return key.Trim();
    }
}
