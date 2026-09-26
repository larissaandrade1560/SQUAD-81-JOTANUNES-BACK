using JotaNunesForms.Application.Convites;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Convites;

public sealed class DefinirSenhaConviteUseCase
{
    private readonly ValidarTokenConviteUseCase _validar;
    private readonly IConviteAcessoRepository _convites;
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;

    public DefinirSenhaConviteUseCase(
        ValidarTokenConviteUseCase validar,
        IConviteAcessoRepository convites,
        IUsuarioRepository usuarios,
        IPasswordHasher passwordHasher)
    {
        _validar = validar;
        _convites = convites;
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
    }

    public async Task ExecuteAsync(
        string rawToken,
        DefinirSenhaConviteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Senha != request.Confirmacao)
        {
            throw new ConviteException("As senhas não conferem.");
        }

        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
        {
            throw new ConviteException("Senha deve ter no mínimo 6 caracteres.");
        }

        var convite = await _validar.ObterConviteUtilizavelAsync(rawToken, cancellationToken);
        var usuario = await _usuarios.GetByIdAsync(convite.UsuarioId, cancellationToken);
        if (usuario is null)
        {
            throw new ConviteException(ValidarTokenConviteUseCase.MensagemTokenInvalido);
        }

        var utcNow = DateTime.UtcNow;
        usuario.AtivarComSenha(_passwordHasher.Hash(request.Senha));
        convite.MarcarUsado(utcNow);

        await _usuarios.UpdateAsync(usuario, cancellationToken);
        await _convites.UpdateAsync(convite, cancellationToken);
    }
}
