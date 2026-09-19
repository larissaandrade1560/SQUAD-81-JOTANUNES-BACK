using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record UpdateUsuarioRequest(
    string NomeExibicao,
    PerfilUsuario Perfil,
    bool Ativo,
    string? Senha,
    Guid? EmpresaId);
