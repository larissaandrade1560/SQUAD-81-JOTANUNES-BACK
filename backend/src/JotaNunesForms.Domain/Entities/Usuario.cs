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

        var digits = new string(documento.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            throw new ArgumentException("Documento inválido.", nameof(documento));
        }

        return digits;
    }

    public void AtualizarPerfil(string nomeExibicao, PerfilUsuario perfil)
    {
        if (string.IsNullOrWhiteSpace(nomeExibicao))
        {
            throw new ArgumentException("Nome de exibição é obrigatório.", nameof(nomeExibicao));
        }

        NomeExibicao = nomeExibicao.Trim();
        Perfil = perfil;
    }

    public void DefinirStatus(bool ativo) => Ativo = ativo;

    public void AlterarSenha(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Hash de senha é obrigatório.", nameof(passwordHash));
        }

        PasswordHash = passwordHash;
    }

    public string PerfilRotulo =>
        Perfil == PerfilUsuario.Administrador ? "Administrador" : "Analista";
}
