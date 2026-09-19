using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record ObraResponse(
    Guid Id,
    string Nome,
    string Codigo,
    string? Cidade,
    string? Uf,
    bool Ativo,
    DateTime CriadoEm)
{
    public static ObraResponse FromEntity(Obra obra) =>
        new(
            obra.Id,
            obra.Nome,
            obra.Codigo,
            obra.Cidade,
            obra.Uf,
            obra.Ativo,
            obra.CriadoEm);
}
