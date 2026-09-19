using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Empresas;

public sealed class UpdateEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;

    public UpdateEmpresaUseCase(IEmpresaRepository empresas) => _empresas = empresas;

    public async Task<EmpresaResponse> ExecuteAsync(
        Guid id,
        UpdateEmpresaRequest request,
        CancellationToken cancellationToken = default)
    {
        var empresa = await _empresas.GetByIdAsync(id, cancellationToken);
        if (empresa is null)
        {
            throw new EmpresaException("Empresa não encontrada.");
        }

        try
        {
            empresa.Atualizar(
                request.RazaoSocial,
                request.Tipo,
                request.NomeFantasia,
                request.EmailContato,
                request.TelefoneContato);
            empresa.DefinirStatus(request.Ativo);

            await _empresas.UpdateAsync(empresa, cancellationToken);
            return EmpresaResponse.FromEntity(empresa);
        }
        catch (ArgumentException ex)
        {
            throw new EmpresaException(ex.Message);
        }
    }
}
