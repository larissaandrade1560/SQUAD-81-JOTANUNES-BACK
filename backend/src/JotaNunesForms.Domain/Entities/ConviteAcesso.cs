namespace JotaNunesForms.Domain.Entities;

public sealed class ConviteAcesso
{
    public Guid Id { get; private set; }

    public Guid EmpresaId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiraEm { get; private set; }

    public DateTime? UsadoEm { get; private set; }

    public DateTime? InvalidadoEm { get; private set; }

    public Guid ConvidadoPorUsuarioId { get; private set; }

    public DateTime CriadoEm { get; private set; }

    private ConviteAcesso()
    {
    }

    public ConviteAcesso(
        Guid empresaId,
        Guid usuarioId,
        string email,
        string tokenHash,
        DateTime expiraEmUtc,
        Guid convidadoPorUsuarioId,
        DateTime criadoEmUtc)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ArgumentException("Empresa é obrigatória.", nameof(empresaId));
        }

        if (usuarioId == Guid.Empty)
        {
            throw new ArgumentException("Usuário é obrigatório.", nameof(usuarioId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("Hash do token é obrigatório.", nameof(tokenHash));
        }

        if (convidadoPorUsuarioId == Guid.Empty)
        {
            throw new ArgumentException("Convidador é obrigatório.", nameof(convidadoPorUsuarioId));
        }

        Id = Guid.NewGuid();
        EmpresaId = empresaId;
        UsuarioId = usuarioId;
        Email = Usuario.NormalizeEmail(email);
        TokenHash = tokenHash;
        ExpiraEm = expiraEmUtc;
        ConvidadoPorUsuarioId = convidadoPorUsuarioId;
        CriadoEm = criadoEmUtc;
    }

    public bool PodeUsar(DateTime utcNow) =>
        UsadoEm is null && InvalidadoEm is null && ExpiraEm > utcNow;

    public void Invalidar(DateTime utcNow) => InvalidadoEm = utcNow;

    public void MarcarUsado(DateTime utcNow) => UsadoEm = utcNow;
}
