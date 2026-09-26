namespace JotaNunesForms.Domain.Entities;

public sealed class Obra
{
    public Guid Id { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Codigo { get; private set; } = string.Empty;

    public string? Cidade { get; private set; }

    public string? Uf { get; private set; }

    public bool Ativo { get; private set; }

    public string? EngenheiroResponsavel { get; private set; }

    public DateOnly? DataInicio { get; private set; }

    public DateOnly? DataFim { get; private set; }

    public string? EquipesInternas { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private Obra()
    {
    }

    public Obra(string nome, string codigo, string? cidade = null, string? uf = null)
    {
        Id = Guid.NewGuid();
        Nome = NormalizeNome(nome);
        Codigo = NormalizeCodigo(codigo);
        Cidade = NormalizeOptional(cidade);
        Uf = NormalizeUf(uf);
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public static string NormalizeCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("Código da obra é obrigatório.", nameof(codigo));
        }

        var normalized = codigo.Trim().ToUpperInvariant();
        if (normalized.Length > 32)
        {
            throw new ArgumentException("Código da obra deve ter no máximo 32 caracteres.", nameof(codigo));
        }

        return normalized;
    }

    public void Atualizar(
        string nome,
        string? cidade,
        string? uf,
        string? engenheiroResponsavel = null,
        DateOnly? dataInicio = null,
        DateOnly? dataFim = null,
        string? equipesInternas = null)
    {
        Nome = NormalizeNome(nome);
        Cidade = NormalizeOptional(cidade);
        Uf = NormalizeUf(uf);
        EngenheiroResponsavel = NormalizeOptional(engenheiroResponsavel);
        DataInicio = dataInicio;
        DataFim = dataFim;
        EquipesInternas = NormalizeOptional(equipesInternas);
    }

    public void DefinirStatus(bool ativo) => Ativo = ativo;

    private static string NormalizeNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome da obra é obrigatório.", nameof(nome));
        }

        return nome.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeUf(string? uf)
    {
        if (string.IsNullOrWhiteSpace(uf))
        {
            return null;
        }

        var letters = new string(uf.Where(char.IsLetter).ToArray()).ToUpperInvariant();
        if (letters.Length != 2)
        {
            throw new ArgumentException("UF deve conter 2 letras.", nameof(uf));
        }

        return letters;
    }
}
