namespace JotaNunesForms.Application.Convites;

public sealed class EmailNotificationException : Exception
{
    public EmailNotificationException()
        : base("Não foi possível enviar o e-mail. Tente novamente.")
    {
    }
}
