using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Obras;

public sealed class CreateObraUseCase
{
    private readonly IObraRepository _obras;

    public CreateObraUseCase(IObraRepository obras) => _obras = obras;

    public async Task<ObraResponse> ExecuteAsync(
        CreateObraRequest request,
        CancellationToken cancellationToken = default)
    {
        string codigo;
        try
        {
            codigo = Obra.NormalizeCodigo(request.Codigo);
        }
        catch (ArgumentException)
        {
            throw new ObraException("Código da obra inválido.");
        }

        if (await _obras.ExistsCodigoAsync(codigo, cancellationToken: cancellationToken))
        {
            throw new ObraException("Já existe uma obra com este código.");
        }

        try
        {
            var obra = new Obra(request.Nome, codigo, request.Cidade, request.Uf);
            await _obras.AddAsync(obra, cancellationToken);
            return ObraResponse.FromEntity(obra);
        }
        catch (ArgumentException ex)
        {
            throw new ObraException(ex.Message);
        }
    }
}
