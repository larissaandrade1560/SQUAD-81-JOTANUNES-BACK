namespace JotaNunesForms.Domain.Entities;

public sealed class Empresa
{
    public Guid Id { get; private set; }

    public string RazaoSocial { get; private set; } = string.Empty;

    public string Cnpj { get; private set; } = string.Empty;

    public string? NomeFantasia { get; private set; }

    public string? EmailContato { get; private set; }

    public string? TelefoneContato { get; private set; }

    public TipoEmpresa Tipo { get; private set; }

    public bool Ativo { get; private set; }

    public string? Logradouro { get; private set; }

    public string? NumeroEndereco { get; private set; }

    public string? Bairro { get; private set; }

    public string? Municipio { get; private set; }

    public string? Uf { get; private set; }

    public string? Cep { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private Empresa()
    {
    }

    public Empresa(
        string razaoSocial,
        string cnpj,
        TipoEmpresa tipo,
        string? nomeFantasia = null,
        string? emailContato = null,
        string? telefoneContato = null)
    {
        Id = Guid.NewGuid();
        RazaoSocial = NormalizeRazaoSocial(razaoSocial);
        Cnpj = NormalizeCnpj(cnpj);
        Tipo = tipo;
        NomeFantasia = NormalizeOptional(nomeFantasia);
        EmailContato = NormalizeOptional(emailContato);
        TelefoneContato = NormalizeOptional(telefoneContato);
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public static string NormalizeCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            throw new ArgumentException("CNPJ é obrigatório.", nameof(cnpj));
        }

        var digits = new string(cnpj.Where(char.IsDigit).ToArray());
        if (digits.Length != 14)
        {
            throw new ArgumentException("CNPJ deve conter 14 dígitos.", nameof(cnpj));
        }

        return digits;
    }

    public string TipoRotulo =>
        Tipo == TipoEmpresa.MaoDeObra ? "Mão de Obra" : "Materiais";

    public void Atualizar(
        string razaoSocial,
        TipoEmpresa tipo,
        string? nomeFantasia,
        string? emailContato,
        string? telefoneContato)
    {
        RazaoSocial = NormalizeRazaoSocial(razaoSocial);
        Tipo = tipo;
        NomeFantasia = NormalizeOptional(nomeFantasia);
        EmailContato = NormalizeOptional(emailContato);
        TelefoneContato = NormalizeOptional(telefoneContato);
    }

    public void DefinirStatus(bool ativo) => Ativo = ativo;

    public void AtualizarEndereco(
        string? logradouro,
        string? numeroEndereco,
        string? bairro,
        string? municipio,
        string? uf,
        string? cep)
    {
        Logradouro = NormalizeOptional(logradouro);
        NumeroEndereco = NormalizeOptional(numeroEndereco);
        Bairro = NormalizeOptional(bairro);
        Municipio = NormalizeOptional(municipio);
        Uf = NormalizeUf(uf);
        Cep = NormalizeCep(cep);
    }

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

    private static string? NormalizeCep(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            return null;
        }

        var digits = new string(cep.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    private static string NormalizeRazaoSocial(string razaoSocial)
    {
        if (string.IsNullOrWhiteSpace(razaoSocial))
        {
            throw new ArgumentException("Razão social é obrigatória.", nameof(razaoSocial));
        }

        return razaoSocial.Trim();
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
