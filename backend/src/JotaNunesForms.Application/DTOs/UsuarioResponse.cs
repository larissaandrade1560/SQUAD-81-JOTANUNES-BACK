using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record UsuarioResponse(
    Guid Id,
    string Documento,
    string NomeExibicao,
    PerfilUsuario Perfil,
    string PerfilRotulo,
    bool Ativo,
    Guid? EmpresaId)
{
    public static UsuarioResponse FromEntity(Usuario usuario) =>
        new(
            usuario.Id,
            usuario.Documento,
            usuario.NomeExibicao,
            usuario.Perfil,
            usuario.PerfilRotulo,
            usuario.Ativo,
            usuario.EmpresaId);
}
