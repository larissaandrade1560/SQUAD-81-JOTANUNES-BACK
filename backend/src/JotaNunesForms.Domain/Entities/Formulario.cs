namespace JotaNunesForms.Domain.Entities;

public sealed class Formulario
{
    public int Id { get; private set; }

    public string Titulo { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private Formulario()
    {
    }

    public Formulario(string titulo, string? descricao = null)
    {
        SetTitulo(titulo);
        Descricao = NormalizeDescricao(descricao);
        CriadoEm = DateTime.UtcNow;
    }

    private void SetTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new ArgumentException("Título é obrigatório.", nameof(titulo));
        }

        Titulo = titulo.Trim();
    }

    private static string? NormalizeDescricao(string? descricao)
    {
        return string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
    }
}
