namespace JotaNunesForms.Application.Documentos;

public sealed class DocumentoEmpresaException : Exception
{
    public int StatusCode { get; }

    public DocumentoEmpresaException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
