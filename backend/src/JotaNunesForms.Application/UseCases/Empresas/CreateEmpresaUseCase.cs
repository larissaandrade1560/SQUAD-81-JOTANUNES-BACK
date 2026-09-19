using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Empresas;

public sealed class CreateEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;

    public CreateEmpresaUseCase(IEmpresaRepository empresas) => _empresas = empresas;

    public async Task<EmpresaResponse> ExecuteAsync(
        CreateEmpresaRequest request,
        CancellationToken cancellationToken = default)
    {
        string cnpj;
        try
        {
            cnpj = Empresa.NormalizeCnpj(request.Cnpj);
        }
        catch (ArgumentException)
        {
            throw new EmpresaException("CNPJ inválido. Informe 14 dígitos.");
        }

        if (await _empresas.ExistsCnpjAsync(cnpj, cancellationToken: cancellationToken))
        {
            throw new EmpresaException("Já existe uma empresa com este CNPJ.");
        }

        try
        {
            var empresa = new Empresa(
                request.RazaoSocial,
                cnpj,
                request.Tipo,
                request.NomeFantasia,
                request.EmailContato,
                request.TelefoneContato);

            await _empresas.AddAsync(empresa, cancellationToken);
            return EmpresaResponse.FromEntity(empresa);
        }
        catch (ArgumentException ex)
        {
            throw new EmpresaException(ex.Message);
        }
    }
}
