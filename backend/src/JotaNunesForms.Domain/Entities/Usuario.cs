namespace JotaNunesForms.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }

    public string Documento { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string NomeExibicao { get; private set; } = string.Empty;

    public PerfilUsuario Perfil { get; private set; }

    public bool Ativo { get; private set; }

    private Usuario()
    {
    }

    public Usuario(
        string documento,
        string passwordHash,
        string nomeExibicao,
        PerfilUsuario perfil)
    {
        Id = Guid.NewGuid();
        Documento = NormalizeDocumento(documento);
        PasswordHash = passwordHash;
        NomeExibicao = nomeExibicao.Trim();
        Perfil = perfil;
        Ativo = true;
    }

    public static string NormalizeDocumento(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
        {
            throw new ArgumentException("Documento é obrigatório.", nameof(documento));
        }

        return documento.Trim();
    }

    public string PerfilRotulo =>
        Perfil == PerfilUsuario.Administrador ? "Administrador" : "Analista";
}
