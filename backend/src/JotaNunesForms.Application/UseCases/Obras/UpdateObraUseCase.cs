using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Obras;

public sealed class UpdateObraUseCase
{
    private readonly IObraRepository _obras;

    public UpdateObraUseCase(IObraRepository obras) => _obras = obras;

    public async Task<ObraResponse> ExecuteAsync(
        Guid id,
        UpdateObraRequest request,
        CancellationToken cancellationToken = default)
    {
        var obra = await _obras.GetByIdAsync(id, cancellationToken);
        if (obra is null)
        {
            throw new ObraException("Obra não encontrada.");
        }

        try
        {
            obra.Atualizar(request.Nome, request.Cidade, request.Uf);
            obra.DefinirStatus(request.Ativo);
            await _obras.UpdateAsync(obra, cancellationToken);
            return ObraResponse.FromEntity(obra);
        }
        catch (ArgumentException ex)
        {
            throw new ObraException(ex.Message);
        }
    }
}
