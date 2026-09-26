namespace JotaNunesForms.Application.Socios;

public sealed class SocioException : Exception
{
    public int StatusCode { get; }

    public SocioException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
