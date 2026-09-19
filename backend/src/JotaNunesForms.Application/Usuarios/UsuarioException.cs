namespace JotaNunesForms.Application.Usuarios;

public sealed class UsuarioException : Exception
{
    public UsuarioException(string message)
        : base(message)
    {
    }
}
