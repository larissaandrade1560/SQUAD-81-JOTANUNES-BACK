namespace JotaNunesForms.Domain.Entities;

public sealed class Socio
{
    public Guid Id { get; private set; }

    public Guid EmpresaId { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Cpf { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private Socio()
    {
    }

    public Socio(Guid empresaId, string nome, string cpf)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ArgumentException("Empresa é obrigatória.", nameof(empresaId));
        }

        Id = Guid.NewGuid();
        EmpresaId = empresaId;
        Nome = NormalizeNome(nome);
        Cpf = NormalizeCpf(cpf);
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public static string NormalizeCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            throw new ArgumentException("CPF é obrigatório.", nameof(cpf));
        }

        var digits = new string(cpf.Where(char.IsDigit).ToArray());
        if (digits.Length != 11)
        {
            throw new ArgumentException("CPF deve conter 11 dígitos.", nameof(cpf));
        }

        return digits;
    }

    public void Atualizar(string nome, bool ativo)
    {
        Nome = NormalizeNome(nome);
        Ativo = ativo;
    }

    private static string NormalizeNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException("Nome é obrigatório.", nameof(nome));
        }

        return nome.Trim();
    }
}
