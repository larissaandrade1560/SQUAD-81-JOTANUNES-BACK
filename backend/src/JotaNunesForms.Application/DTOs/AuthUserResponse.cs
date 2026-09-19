using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record AuthUserResponse(
    string Documento,
    string NomeExibicao,
    string PerfilRotulo,
    PerfilUsuario Perfil)
{
    public static AuthUserResponse FromUsuario(Usuario usuario) =>
        new(usuario.Documento, usuario.NomeExibicao, usuario.PerfilRotulo, usuario.Perfil);
}
