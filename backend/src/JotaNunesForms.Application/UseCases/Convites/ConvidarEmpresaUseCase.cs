using JotaNunesForms.Application.Convites;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.Extensions.Configuration;

namespace JotaNunesForms.Application.UseCases.Convites;

public sealed class ConvidarEmpresaUseCase
{
    private static readonly TimeSpan ValidadeConvite = TimeSpan.FromHours(48);

    private readonly IEmpresaRepository _empresas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IConviteAcessoRepository _convites;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public ConvidarEmpresaUseCase(
        IEmpresaRepository empresas,
        IUsuarioRepository usuarios,
        IConviteAcessoRepository convites,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _empresas = empresas;
        _usuarios = usuarios;
        _convites = convites;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<(ConviteAcessoResponse Response, bool Created)> ExecuteAsync(
        Guid empresaId,
        ConvidarEmpresaRequest request,
        Guid convidadoPorUsuarioId,
        CancellationToken cancellationToken = default)
    {
        string email;
        try
        {
            email = Usuario.NormalizeEmail(request.Email);
        }
        catch (ArgumentException)
        {
            throw new ConviteException("Informe um e-mail válido.");
        }

        var empresa = await _empresas.GetByIdAsync(empresaId, cancellationToken);
        if (empresa is null)
        {
            throw new EmpresaException("Empresa não encontrada.");
        }

        if (!empresa.Ativo)
        {
            throw new EmpresaException("Empresa inativa não pode receber convite.");
        }

        var usuario = await _usuarios.GetTerceirizadoByEmpresaAsync(empresaId, cancellationToken);
        var created = false;
        if (usuario is null)
        {
            usuario = Usuario.CriarTerceirizadoParaConvite(
                empresa.Cnpj,
                empresa.RazaoSocial,
                empresaId);
            await _usuarios.AddAsync(usuario, cancellationToken);
            created = true;
        }

        if (await _usuarios.ExistsEmailAsync(email, usuario.Id, cancellationToken))
        {
            throw new ConviteException("Este e-mail já está em uso por outra empresa.");
        }

        usuario.DefinirEmailConvite(email);
        await _usuarios.UpdateAsync(usuario, cancellationToken);

        var utcNow = DateTime.UtcNow;
        await _convites.InvalidateUnusedForUsuarioAsync(usuario.Id, utcNow, cancellationToken);

        var (rawToken, tokenHash) = ConviteToken.CreateRawAndHash();
        var expiraEm = utcNow.Add(ValidadeConvite);
        var convite = new ConviteAcesso(
            empresaId,
            usuario.Id,
            email,
            tokenHash,
            expiraEm,
            convidadoPorUsuarioId,
            utcNow);

        await _convites.AddAsync(convite, cancellationToken);

        var publicBaseUrl = _configuration["App:PublicBaseUrl"]
            ?? throw new InvalidOperationException("App:PublicBaseUrl não configurada.");

        var (textBody, htmlBody) = ConviteEmailComposer.Compose(
            publicBaseUrl,
            empresa.RazaoSocial,
            rawToken);

        try
        {
            await _emailSender.SendAsync(
                email,
                ConviteEmailComposer.Subject,
                textBody,
                htmlBody,
                CancellationToken.None);
        }
        catch
        {
            convite.Invalidar(utcNow);
            await _convites.UpdateAsync(convite, cancellationToken);
            throw new EmailNotificationException();
        }

        var response = new ConviteAcessoResponse(
            empresaId,
            email,
            "Pendente",
            expiraEm);

        return (response, created);
    }
}
