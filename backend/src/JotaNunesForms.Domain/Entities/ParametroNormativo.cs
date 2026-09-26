namespace JotaNunesForms.Domain.Entities;

public sealed class ParametroNormativo
{
    public Guid Id { get; private set; }

    public string Chave { get; private set; } = string.Empty;

    public string Valor { get; private set; } = string.Empty;

    public string Unidade { get; private set; } = string.Empty;

    public DateTime AtualizadoEm { get; private set; }

    private ParametroNormativo()
    {
    }

    public ParametroNormativo(string chave, string valor, string unidade)
        : this(Guid.NewGuid(), chave, valor, unidade)
    {
    }

    public ParametroNormativo(Guid id, string chave, string valor, string unidade)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id é obrigatório.", nameof(id));
        }

        Id = id;
        Chave = NormalizeChave(chave);
        Unidade = NormalizeObrigatorio(unidade, nameof(unidade));
        DefinirValor(valor);
    }

    public void DefinirValor(string valor)
    {
        Valor = NormalizeObrigatorio(valor, nameof(valor));
        AtualizadoEm = DateTime.UtcNow;
    }

    private static string NormalizeChave(string chave)
    {
        if (string.IsNullOrWhiteSpace(chave))
        {
            throw new ArgumentException("Chave é obrigatória.", nameof(chave));
        }

        return chave.Trim().ToUpperInvariant();
    }

    private static string NormalizeObrigatorio(string value, string param)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Valor obrigatório.", param);
        }

        return value.Trim();
    }
}
