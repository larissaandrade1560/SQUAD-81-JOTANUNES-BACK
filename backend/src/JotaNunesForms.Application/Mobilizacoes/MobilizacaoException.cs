namespace JotaNunesForms.Application.Mobilizacoes;

public sealed class MobilizacaoException : Exception
{
    public int StatusCode { get; }

    public MobilizacaoException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
