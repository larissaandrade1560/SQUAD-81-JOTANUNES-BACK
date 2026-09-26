namespace JotaNunesForms.Application.Convites;

public static class ConviteEmailComposer
{
    public const string Subject = "Acesso ao Portal de Terceirizadas";

    public static (string TextBody, string HtmlBody) Compose(
        string publicBaseUrl,
        string razaoSocial,
        string rawToken)
    {
        var link = $"{publicBaseUrl.TrimEnd('/')}/definir-senha?token={rawToken}";

        var textBody =
            $"Olá,\n\n" +
            $"A Jotanunes convidou a empresa {razaoSocial} para acessar o Portal de Terceirizadas.\n\n" +
            $"Defina sua senha pelo link (válido por 48 horas):\n{link}\n\n" +
            $"Se você não esperava este convite, ignore este e-mail.";

        var htmlBody =
            $"""
            <p>Olá,</p>
            <p>A <strong>Jotanunes</strong> convidou a empresa <strong>{razaoSocial}</strong> para acessar o <strong>Portal de Terceirizadas</strong>.</p>
            <p><a href="{link}">Definir senha de acesso</a></p>
            <p>O link é válido por <strong>48 horas</strong>.</p>
            <p>Se você não esperava este convite, ignore este e-mail.</p>
            """;

        return (textBody, htmlBody);
    }
}
