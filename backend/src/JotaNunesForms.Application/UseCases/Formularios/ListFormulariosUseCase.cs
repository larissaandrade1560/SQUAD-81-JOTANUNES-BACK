using JotaNunesForms.Application.DTOs;
using JotaNunesForms.Domain.Ports;

namespace JotaNunesForms.Application.UseCases.Formularios;

public sealed class ListFormulariosUseCase
{
    private readonly IFormularioRepository _formularios;

    public ListFormulariosUseCase(IFormularioRepository formularios)
    {
        _formularios = formularios;
    }

    public async Task<IReadOnlyList<FormularioResponse>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var formularios = await _formularios.ListAsync(cancellationToken);

        return formularios
            .Select(FormularioResponse.FromDomain)
            .ToList();
    }
}
