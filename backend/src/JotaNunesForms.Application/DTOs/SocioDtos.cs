using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record CreateSocioRequest(string Nome, string Cpf);

public sealed record UpdateSocioRequest(string Nome, bool Ativo);

public sealed record SocioResponse(
    Guid Id,
    Guid EmpresaId,
    string Nome,
    string Cpf,
    bool Ativo,
    DateTime CriadoEm)
{
    public static SocioResponse FromEntity(Socio socio) =>
        new(socio.Id, socio.EmpresaId, socio.Nome, socio.Cpf, socio.Ativo, socio.CriadoEm);
}
