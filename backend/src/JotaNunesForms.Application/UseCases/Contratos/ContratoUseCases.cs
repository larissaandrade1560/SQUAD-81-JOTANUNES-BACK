using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Application.Empresas;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Application.Processos;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Contratos;

public sealed class CreateContratoUseCase
{
    private readonly IContratoRepository _contratos;
    private readonly IEmpresaRepository _empresas;
    private readonly IObraRepository _obras;

    public CreateContratoUseCase(
        IContratoRepository contratos,
        IEmpresaRepository empresas,
        IObraRepository obras)
    {
        _contratos = contratos;
        _empresas = empresas;
        _obras = obras;
    }

    public async Task<ContratoResponse> ExecuteAsync(
        CreateContratoRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = await _empresas.GetByIdAsync(request.EmpresaId, cancellationToken)
            ?? throw new EmpresaException("Empresa não encontrada.");
        _ = await _obras.GetByIdAsync(request.ObraId, cancellationToken)
            ?? throw new ObraException("Obra não encontrada.");

        try
        {
            var contrato = new Contrato(request.EmpresaId, request.ObraId, request.EscopoServico, request.Numero);
            await _contratos.AddAsync(contrato, cancellationToken);
            return ContratoResponse.FromEntity(contrato);
        }
        catch (ArgumentException ex)
        {
            throw new ProcessoException(ex.Message);
        }
    }
}

public sealed class ListContratosUseCase
{
    private readonly IContratoRepository _contratos;

    public ListContratosUseCase(IContratoRepository contratos) => _contratos = contratos;

    public async Task<IReadOnlyList<ContratoResponse>> ExecuteAsync(
        AccessScope scope,
        Guid? empresaFilterId,
        Guid? obraId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = scope switch
        {
            AccessScope.Internal => empresaFilterId,
            AccessScope.Company company => company.CompanyId,
            _ => throw new InvalidIdentityException("Escopo de acesso inválido."),
        };

        var contratos = await _contratos.ListAsync(empresaId, obraId, cancellationToken);
        return contratos.Select(ContratoResponse.FromEntity).ToList();
    }
}
