using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Obras;

public sealed class ListObrasUseCase
{
    private readonly IObraRepository _obras;

    public ListObrasUseCase(IObraRepository obras) => _obras = obras;

    public async Task<IReadOnlyList<ObraResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var list = await _obras.ListAsync(cancellationToken);
        return list.Select(ObraResponse.FromEntity).ToList();
    }
}
