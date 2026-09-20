namespace JotaNunesForms.Application.Pagamentos;

public sealed class PagamentoException : Exception
{
    public PagamentoException(string message)
        : base(message)
    {
    }
}
