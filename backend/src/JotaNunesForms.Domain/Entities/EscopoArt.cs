namespace JotaNunesForms.Domain.Entities;

public sealed class EscopoArt
{
    public Guid Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    private EscopoArt()
    {
    }

    public EscopoArt(string nome)
        : this(Guid.NewGuid(), nome)
    {
    }

    public EscopoArt(Guid id, string nome)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id é obrigatório.", nameof(id));
        }

        Id = id;
        DefinirNome(nome);
        Ativo = true;
    }

    public void DefinirNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        }

        Nome = nome.Trim();
    }

    public void DefinirStatus(bool ativo) => Ativo = ativo;
}
