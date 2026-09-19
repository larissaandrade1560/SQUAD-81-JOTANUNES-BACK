using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Auth;

public sealed class GetAuthenticatedUserUseCase
{
    private readonly IUsuarioRepository _usuarios;

    public GetAuthenticatedUserUseCase(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<AuthUserResponse> ExecuteAsync(
        string documento,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarios.GetByDocumentoAsync(documento, cancellationToken);

        if (usuario is null || !usuario.Ativo)
        {
            throw new AuthException("Sessão inválida.");
        }

        return AuthUserResponse.FromUsuario(usuario);
    }
}
