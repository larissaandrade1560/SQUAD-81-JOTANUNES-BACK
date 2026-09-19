using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Application.DTOs;

public sealed record FuncionarioResponse(
    Guid Id,
    Guid EmpresaId,
    string EmpresaRazaoSocial,
    string Nome,
    string Cpf,
    string Cargo,
    bool Ativo,
    DateTime CriadoEm)
{
    public static FuncionarioResponse FromEntity(Funcionario funcionario, string empresaRazaoSocial) =>
        new(
            funcionario.Id,
            funcionario.EmpresaId,
            empresaRazaoSocial,
            funcionario.Nome,
            funcionario.Cpf,
            funcionario.Cargo,
            funcionario.Ativo,
            funcionario.CriadoEm);
}
