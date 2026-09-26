using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Application.Auth;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Obras;

public sealed class ListObrasUseCase
{
    private readonly IObraRepository _obras;

    public ListObrasUseCase(IObraRepository obras) => _obras = obras;

    public async Task<IReadOnlyList<ObraResponse>> ExecuteAsync(
        AccessScope scope,
        CancellationToken cancellationToken = default)
    {
        var list = await _obras.ListAsync(cancellationToken);
        if (scope is AccessScope.Company)
        {
            list = list.Where(obra => obra.Ativo).ToList();
        }

        return list.Select(ObraResponse.FromEntity).ToList();
    }
}
