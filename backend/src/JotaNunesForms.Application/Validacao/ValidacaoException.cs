namespace JotaNunesForms.Application.Validacao;

public sealed class ValidacaoException : Exception
{
    public ValidacaoException(string message)
        : base(message)
    {
    }
}
