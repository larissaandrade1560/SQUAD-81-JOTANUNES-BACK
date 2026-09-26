namespace JotaNunesForms.Domain.Entities;

public sealed class DocumentoVersao
{
    public Guid Id { get; private set; }

    public Guid ItemChecklistId { get; private set; }

    public int Numero { get; private set; }

    public string? NomeArquivo { get; private set; }

    public string? StorageKey { get; private set; }

    public string? ContentType { get; private set; }

    public long? TamanhoBytes { get; private set; }

    public string? HashSha256 { get; private set; }

    public string? CamposJson { get; private set; }

    public Guid EnviadoPorUsuarioId { get; private set; }

    public DateTime EnviadoEm { get; private set; }

    public bool Vigente { get; private set; }

    private DocumentoVersao()
    {
    }

    public DocumentoVersao(
        Guid itemChecklistId,
        int numero,
        Guid enviadoPorUsuarioId,
        string? nomeArquivo = null,
        string? storageKey = null,
        string? contentType = null,
        long? tamanhoBytes = null,
        string? hashSha256 = null,
        string? camposJson = null,
        Guid? id = null)
    {
        if (itemChecklistId == Guid.Empty)
        {
            throw new ArgumentException("Item de checklist é obrigatório.", nameof(itemChecklistId));
        }

        if (numero < 1)
        {
            throw new ArgumentException("Número da versão deve ser positivo.", nameof(numero));
        }

        if (enviadoPorUsuarioId == Guid.Empty)
        {
            throw new ArgumentException("Remetente é obrigatório.", nameof(enviadoPorUsuarioId));
        }

        Id = id ?? Guid.NewGuid();
        ItemChecklistId = itemChecklistId;
        Numero = numero;
        NomeArquivo = NormalizeOptional(nomeArquivo);
        StorageKey = NormalizeOptional(storageKey);
        ContentType = NormalizeOptional(contentType);
        TamanhoBytes = tamanhoBytes;
        HashSha256 = NormalizeOptional(hashSha256);
        CamposJson = NormalizeOptional(camposJson);
        EnviadoPorUsuarioId = enviadoPorUsuarioId;
        EnviadoEm = DateTime.UtcNow;
        Vigente = true;
    }

    public void MarcarNaoVigente() => Vigente = false;

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
