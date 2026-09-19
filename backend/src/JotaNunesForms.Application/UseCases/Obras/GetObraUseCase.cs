using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Obras;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Obras;

public sealed class GetObraUseCase
{
    private readonly IObraRepository _obras;

    public GetObraUseCase(IObraRepository obras) => _obras = obras;

    public async Task<ObraResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var obra = await _obras.GetByIdAsync(id, cancellationToken);
        if (obra is null)
        {
            throw new ObraException("Obra não encontrada.");
        }

        return ObraResponse.FromEntity(obra);
    }
}
