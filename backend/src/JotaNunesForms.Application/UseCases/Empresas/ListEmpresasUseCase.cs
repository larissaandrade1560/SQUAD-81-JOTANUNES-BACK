using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Empresas;

public sealed class ListEmpresasUseCase
{
    private readonly IEmpresaRepository _empresas;

    public ListEmpresasUseCase(IEmpresaRepository empresas) => _empresas = empresas;

    public async Task<IReadOnlyList<EmpresaResponse>> ExecuteAsync(
        AccessScope scope,
        CancellationToken cancellationToken = default)
    {
        if (scope is AccessScope.Company companyScope)
        {
            var ownCompany = await _empresas.GetByIdAsync(companyScope.CompanyId, cancellationToken);
            return ownCompany is null ? [] : [EmpresaResponse.FromEntity(ownCompany)];
        }

        var list = await _empresas.ListAsync(cancellationToken);
        return list.Select(EmpresaResponse.FromEntity).ToList();
    }
}
