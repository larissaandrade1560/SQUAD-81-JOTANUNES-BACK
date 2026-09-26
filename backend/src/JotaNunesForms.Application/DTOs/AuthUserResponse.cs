using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record AuthUserResponse(
    Guid Id,
    string Documento,
    string NomeExibicao,
    string PerfilRotulo,
    PerfilUsuario Perfil)
{
    public static AuthUserResponse FromUsuario(Usuario usuario) =>
        new(usuario.Id, usuario.Documento, usuario.NomeExibicao, usuario.PerfilRotulo, usuario.Perfil);
}
