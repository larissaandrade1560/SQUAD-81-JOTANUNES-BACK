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
    DateTime CriadoEm,
    IReadOnlyList<FuncionarioObraResumo> Obras)
{
    public static FuncionarioResponse FromEntity(
        Funcionario funcionario,
        string empresaRazaoSocial,
        IReadOnlyList<FuncionarioObraResumo>? obras = null) =>
        new(
            funcionario.Id,
            funcionario.EmpresaId,
            empresaRazaoSocial,
            funcionario.Nome,
            funcionario.Cpf,
            funcionario.Cargo,
            funcionario.Ativo,
            funcionario.CriadoEm,
            obras ?? Array.Empty<FuncionarioObraResumo>());

    public static IReadOnlyList<FuncionarioObraResumo> MapObras(IEnumerable<Obra> obras) =>
        obras
            .OrderBy(o => o.Codigo)
            .Select(o => new FuncionarioObraResumo(o.Id, o.Codigo, o.Nome))
            .ToList();
}
