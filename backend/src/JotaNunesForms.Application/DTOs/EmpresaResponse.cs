using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record EmpresaResponse(
    Guid Id,
    string RazaoSocial,
    string Cnpj,
    string? NomeFantasia,
    string? EmailContato,
    string? TelefoneContato,
    TipoEmpresa Tipo,
    string TipoRotulo,
    bool Ativo,
    DateTime CriadoEm)
{
    public static EmpresaResponse FromEntity(Empresa empresa) =>
        new(
            empresa.Id,
            empresa.RazaoSocial,
            empresa.Cnpj,
            empresa.NomeFantasia,
            empresa.EmailContato,
            empresa.TelefoneContato,
            empresa.Tipo,
            empresa.TipoRotulo,
            empresa.Ativo,
            empresa.CriadoEm);
}
