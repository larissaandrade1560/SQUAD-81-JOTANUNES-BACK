using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record FormularioResponse(
    int Id,
    string Titulo,
    string? Descricao,
    DateTime CriadoEm)
{
    public static FormularioResponse FromDomain(Formulario formulario) =>
        new(formulario.Id, formulario.Titulo, formulario.Descricao, formulario.CriadoEm);
}
