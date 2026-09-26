namespace JotaNunesForms.Application.Documentos;

public sealed class DocumentoFuncionarioException : Exception
{
    public int StatusCode { get; }

    public DocumentoFuncionarioException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
