namespace JotaNunesForms.Application.Processos;

public sealed class ProcessoException : Exception
{
    public ProcessoException(string message)
        : base(message)
    {
    }
}
