namespace JotaNunesForms.Models;

public class Formulario
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}