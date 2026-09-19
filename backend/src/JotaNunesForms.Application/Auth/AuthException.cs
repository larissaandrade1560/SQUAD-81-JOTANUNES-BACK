namespace JotaNunesForms.Application.Auth;

public sealed class AuthException : Exception
{
    public AuthException(string message) : base(message)
    {
    }
}
