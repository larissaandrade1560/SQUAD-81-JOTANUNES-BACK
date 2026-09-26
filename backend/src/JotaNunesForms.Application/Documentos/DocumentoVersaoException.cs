namespace JotaNunesForms.Application.Documentos;

public sealed class DocumentoVersaoException : Exception
{
    public int StatusCode { get; }

    public DocumentoVersaoException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
