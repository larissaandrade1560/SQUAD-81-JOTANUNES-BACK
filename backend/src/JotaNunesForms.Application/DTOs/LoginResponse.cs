using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record LoginResponse(
    string AccessToken,
    string Documento,
    string NomeExibicao,
    string PerfilRotulo,
    PerfilUsuario Perfil,
    DateTime ExpiresAtUtc)
{
    public static LoginResponse Create(
        string accessToken,
        Usuario usuario,
        DateTime expiresAtUtc) =>
        new(
            accessToken,
            usuario.Documento,
            usuario.NomeExibicao,
            usuario.PerfilRotulo,
            usuario.Perfil,
            expiresAtUtc);
}
