namespace JotaNunesForms.Domain.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }

    public string Documento { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public string? PasswordHash { get; private set; }

    public string NomeExibicao { get; private set; } = string.Empty;

    public PerfilUsuario Perfil { get; private set; }

    public bool Ativo { get; private set; }

    public Guid? EmpresaId { get; private set; }

    private Usuario()
    {
    }

    public Usuario(
        string documento,
        string passwordHash,
        string nomeExibicao,
        PerfilUsuario perfil,
        Guid? empresaId = null)
    {
        Id = Guid.NewGuid();
        Documento = NormalizeDocumento(documento);
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? null : passwordHash;
        NomeExibicao = nomeExibicao.Trim();
        Perfil = perfil;
        DefinirVinculoEmpresa(perfil, empresaId);
        Ativo = perfil != PerfilUsuario.Terceirizado || PasswordHash is not null;
    }

    public static Usuario CriarTerceirizadoParaConvite(
        string cnpj,
        string nomeExibicao,
        Guid empresaId) =>
        new(cnpj, string.Empty, nomeExibicao, PerfilUsuario.Terceirizado, empresaId);

    public bool UsaLoginPorEmail =>
        Perfil == PerfilUsuario.Terceirizado && !string.IsNullOrWhiteSpace(Email);

    public static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("E-mail é obrigatório.", nameof(email));
        }

        var normalized = email.Trim().ToLowerInvariant();
        var at = normalized.IndexOf('@');
        if (at <= 0 || at == normalized.Length - 1 || normalized.IndexOf('@', at + 1) >= 0)
        {
            throw new ArgumentException("E-mail inválido.", nameof(email));
        }

        var domain = normalized[(at + 1)..];
        if (!domain.Contains('.', StringComparison.Ordinal))
        {
            throw new ArgumentException("E-mail inválido.", nameof(email));
        }

        return normalized;
    }

    public void DefinirEmailConvite(string email) =>
        Email = NormalizeEmail(email);

    public void AtivarComSenha(string passwordHash)
    {
        AlterarSenha(passwordHash);
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

    public void AtualizarPerfil(string nomeExibicao, PerfilUsuario perfil, Guid? empresaId = null)
    {
        if (string.IsNullOrWhiteSpace(nomeExibicao))
        {
            throw new ArgumentException("Nome de exibição é obrigatório.", nameof(nomeExibicao));
        }

        NomeExibicao = nomeExibicao.Trim();
        Perfil = perfil;
        DefinirVinculoEmpresa(perfil, empresaId);
    }

    public void DefinirVinculoEmpresa(PerfilUsuario perfil, Guid? empresaId)
    {
        if (perfil == PerfilUsuario.Terceirizado)
        {
            if (empresaId is null || empresaId == Guid.Empty)
            {
                throw new ArgumentException("Empresa é obrigatória para usuário terceirizado.", nameof(empresaId));
            }

            EmpresaId = empresaId;
            return;
        }

        EmpresaId = null;
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

    public bool PossuiSenhaDefinida => !string.IsNullOrEmpty(PasswordHash);

    public string PerfilRotulo =>
        Perfil switch
        {
            PerfilUsuario.Administrador => "Administrador",
            PerfilUsuario.Terceirizado => "Terceirizado",
            _ => "Analista",
        };
}
