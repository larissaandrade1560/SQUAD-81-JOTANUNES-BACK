namespace JotaNunesForms.Application.Documentos;

public sealed class DocumentoFuncionarioException : Exception
{
    public DocumentoFuncionarioException(string message)
        : base(message)
    {
    }
}
