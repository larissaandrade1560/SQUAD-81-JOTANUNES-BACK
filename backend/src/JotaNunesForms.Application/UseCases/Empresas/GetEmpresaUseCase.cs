using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Empresas;

public sealed class GetEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly AccessScopeGuard _scopeGuard;

    public GetEmpresaUseCase(IEmpresaRepository empresas, AccessScopeGuard scopeGuard)
    {
        _empresas = empresas;
        _scopeGuard = scopeGuard;
    }

    public async Task<EmpresaResponse> ExecuteAsync(
        Guid id,
        AccessScope scope,
        SecurityRequestContext request,
        CancellationToken cancellationToken = default)
    {
        if (!await _scopeGuard.AllowsCompanyAsync(scope, id, request, cancellationToken))
        {
            throw new EmpresaException("Empresa não encontrada.");
        }

        var empresa = await _empresas.GetByIdAsync(id, cancellationToken);
        if (empresa is null)
        {
            throw new EmpresaException("Empresa não encontrada.");
        }

        return EmpresaResponse.FromEntity(empresa);
    }
}
