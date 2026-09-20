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

    public string? MotivoRejeicao { get; private set; }

    public DateTime? AnalisadoEm { get; private set; }

    public DateTime? ValidoAte { get; private set; }

    public void IniciarAnalise()
    {
        if (Status is not StatusDocumento.Pendente)
        {
            return;
        }

        Status = StatusDocumento.EmAnalise;
    }

    public void Aprovar()
    {
        if (Status is not StatusDocumento.Pendente and not StatusDocumento.EmAnalise)
        {
            throw new InvalidOperationException("Somente documentos pendentes podem ser aprovados.");
        }

        var agora = DateTime.UtcNow;
        Status = StatusDocumento.Aprovado;
        MotivoRejeicao = null;
        AnalisadoEm = agora;
        ValidoAte = CalcularValidoAte(agora);
    }

    public void Rejeitar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("Motivo da rejeição é obrigatório.", nameof(motivo));
        }

        if (Status is not StatusDocumento.Pendente and not StatusDocumento.EmAnalise)
        {
            throw new InvalidOperationException("Somente documentos pendentes podem ser rejeitados.");
        }

        Status = StatusDocumento.Rejeitado;
        MotivoRejeicao = motivo.Trim();
        AnalisadoEm = DateTime.UtcNow;
        ValidoAte = null;
    }

    public void Reenviar(string nomeArquivo, string storageKey, string contentType, long tamanhoBytes)
    {
        if (Status is not StatusDocumento.Rejeitado and not StatusDocumento.Vencido)
        {
            throw new InvalidOperationException(
                "Somente documentos rejeitados ou vencidos podem ser substituídos.");
        }

        SubstituirArquivo(nomeArquivo, storageKey, contentType, tamanhoBytes);
    }

    public void AtualizarVencimentoSeExpirado(DateTime utcNow)
    {
        if (Status is not StatusDocumento.Aprovado || ValidoAte is null)
        {
            return;
        }

        if (ValidoAte.Value <= utcNow)
        {
            Status = StatusDocumento.Vencido;
        }
    }

    private void SubstituirArquivo(string nomeArquivo, string storageKey, string contentType, long tamanhoBytes)
    {
        if (tamanhoBytes <= 0)
        {
            throw new ArgumentException("Arquivo inválido.", nameof(tamanhoBytes));
        }

        NomeArquivo = NormalizeNomeArquivo(nomeArquivo);
        StorageKey = NormalizeStorageKey(storageKey);
        ContentType = contentType;
        TamanhoBytes = tamanhoBytes;
        Status = StatusDocumento.Pendente;
        MotivoRejeicao = null;
        AnalisadoEm = null;
        ValidoAte = null;
        EnviadoEm = DateTime.UtcNow;
    }

    private DateTime CalcularValidoAte(DateTime aprovadoEm) =>
        Tipo switch
        {
            TipoDocumentoEmpresarial.CertidaoNegativa => aprovadoEm.AddDays(90),
            _ => aprovadoEm.AddDays(365),
        };

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
