using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Usuarios;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Usuarios;

public sealed class GetUsuarioUseCase
{
    private readonly IUsuarioRepository _usuarios;

    public GetUsuarioUseCase(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<UsuarioResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarios.GetByIdAsync(id, cancellationToken);
        if (usuario is null)
        {
            throw new UsuarioException("Usuário não encontrado.");
        }

        return UsuarioResponse.FromEntity(usuario);
    }
}
