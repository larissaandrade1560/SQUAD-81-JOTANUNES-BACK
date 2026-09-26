namespace JotaNunesForms.Application.Pagamentos;

public sealed class PagamentoException : Exception
{
    public int StatusCode { get; }

    public PagamentoException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
