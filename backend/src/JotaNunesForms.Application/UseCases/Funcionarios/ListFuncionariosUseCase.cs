using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Funcionarios;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Funcionarios;

public sealed class ListFuncionariosUseCase
{
    private readonly IFuncionarioRepository _funcionarios;
    private readonly IEmpresaRepository _empresas;

    public ListFuncionariosUseCase(IFuncionarioRepository funcionarios, IEmpresaRepository empresas)
    {
        _funcionarios = funcionarios;
        _empresas = empresas;
    }

    public async Task<IReadOnlyList<FuncionarioResponse>> ExecuteAsync(
        Guid? scopeEmpresaId,
        CancellationToken cancellationToken = default)
    {
        var list = await _funcionarios.ListAsync(scopeEmpresaId, cancellationToken);
        var empresas = await _empresas.ListAsync(cancellationToken);
        var nomes = empresas.ToDictionary(e => e.Id, e => e.RazaoSocial);

        return list
            .Select(f => FuncionarioResponse.FromEntity(
                f,
                nomes.GetValueOrDefault(f.EmpresaId, "—")))
            .ToList();
    }
}
