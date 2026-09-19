using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Usuarios;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Usuarios;

public sealed class ListUsuariosUseCase
{
    private readonly IUsuarioRepository _usuarios;

    public ListUsuariosUseCase(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<IReadOnlyList<UsuarioResponse>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _usuarios.ListAsync(cancellationToken);
        return list.Select(UsuarioResponse.FromEntity).ToList();
    }
}
