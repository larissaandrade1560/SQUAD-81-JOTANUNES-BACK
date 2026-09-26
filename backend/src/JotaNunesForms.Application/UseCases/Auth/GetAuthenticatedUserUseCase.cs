using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Entities;
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
        string subject,
        CancellationToken cancellationToken = default)
    {
        Usuario? usuario;
        if (subject.Contains('@', StringComparison.Ordinal))
        {
            try
            {
                var email = Usuario.NormalizeEmail(subject);
                usuario = await _usuarios.GetByEmailAsync(email, cancellationToken);
            }
            catch (ArgumentException)
            {
                throw new AuthException("Sessão inválida.");
            }
        }
        else
        {
            usuario = await _usuarios.GetByDocumentoAsync(subject, cancellationToken);
        }

        if (usuario is null || !usuario.Ativo)
        {
            throw new AuthException("Sessão inválida.");
        }

        return AuthUserResponse.FromUsuario(usuario);
    }

    public async Task<AuthUserResponse> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var state = await _usuarios.GetSecurityStateByIdAsync(userId, cancellationToken);
        if (state is null || !state.Usuario.Ativo
            || (state.Usuario.Perfil == PerfilUsuario.Terceirizado && state.EmpresaAtiva != true))
        {
            throw new AuthException("Sessão inválida.");
        }

        return AuthUserResponse.FromUsuario(state.Usuario, state.EmpresaTipo);
    }
}
