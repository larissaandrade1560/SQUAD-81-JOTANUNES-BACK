namespace JotaNunesForms.Application.Validacao;

public sealed class ValidacaoException : Exception
{
    public int StatusCode { get; }

    public ValidacaoException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
