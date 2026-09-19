namespace JotaNunesForms.Domain.Entities;

public sealed class DocumentoFuncionario
{
    public Guid Id { get; private set; }

    public Guid FuncionarioId { get; private set; }

    public TipoDocumentoFuncionario Tipo { get; private set; }

    public string NomeArquivo { get; private set; } = string.Empty;

    public string StorageKey { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public long TamanhoBytes { get; private set; }

    public StatusDocumento Status { get; private set; }

    public DateTime EnviadoEm { get; private set; }

    private DocumentoFuncionario()
    {
    }

    public DocumentoFuncionario(
        Guid funcionarioId,
        TipoDocumentoFuncionario tipo,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes)
        : this(Guid.NewGuid(), funcionarioId, tipo, nomeArquivo, storageKey, contentType, tamanhoBytes)
    {
    }

    public DocumentoFuncionario(
        Guid id,
        Guid funcionarioId,
        TipoDocumentoFuncionario tipo,
        string nomeArquivo,
        string storageKey,
        string contentType,
        long tamanhoBytes)
    {
        if (funcionarioId == Guid.Empty)
        {
            throw new ArgumentException("Funcionário é obrigatório.", nameof(funcionarioId));
        }

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id é obrigatório.", nameof(id));
        }

        Id = id;
        FuncionarioId = funcionarioId;
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
