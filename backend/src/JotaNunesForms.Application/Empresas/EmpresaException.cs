namespace JotaNunesForms.Application.Empresas;

public sealed class EmpresaException : Exception
{
    public EmpresaException(string message)
        : base(message)
    {
    }
}
