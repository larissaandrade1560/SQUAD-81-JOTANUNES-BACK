namespace JotaNunesForms.Application.Funcionarios;

public sealed class FuncionarioException : Exception
{
    public int StatusCode { get; }

    public FuncionarioException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
