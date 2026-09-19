using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Empresas;

public sealed class ListEmpresasUseCase
{
    private readonly IEmpresaRepository _empresas;

    public ListEmpresasUseCase(IEmpresaRepository empresas) => _empresas = empresas;

    public async Task<IReadOnlyList<EmpresaResponse>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var list = await _empresas.ListAsync(cancellationToken);
        return list.Select(EmpresaResponse.FromEntity).ToList();
    }
}
