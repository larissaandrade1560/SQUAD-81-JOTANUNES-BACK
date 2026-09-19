using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IFormularioRepository
{
    Task<IReadOnlyList<Formulario>> ListAsync(CancellationToken cancellationToken = default);
}
