using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Empresas;

public sealed class GetEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;

    public GetEmpresaUseCase(IEmpresaRepository empresas) => _empresas = empresas;

    public async Task<EmpresaResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empresa = await _empresas.GetByIdAsync(id, cancellationToken);
        if (empresa is null)
        {
            throw new EmpresaException("Empresa não encontrada.");
        }

        return EmpresaResponse.FromEntity(empresa);
    }
}
