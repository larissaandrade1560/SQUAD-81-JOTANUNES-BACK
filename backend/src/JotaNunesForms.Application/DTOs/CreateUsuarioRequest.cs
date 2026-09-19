using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record CreateUsuarioRequest(
    string Documento,
    string NomeExibicao,
    PerfilUsuario Perfil,
    string Senha,
    Guid? EmpresaId);
