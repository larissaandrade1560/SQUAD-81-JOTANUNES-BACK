using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IFuncionarioObraRepository
{
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<Obra>>> ListObrasByFuncionarioIdsAsync(
        IReadOnlyCollection<Guid> funcionarioIds,
        CancellationToken cancellationToken = default);

    Task ReplaceForFuncionarioAsync(
        Guid funcionarioId,
        IReadOnlyList<Guid> obraIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Funcionario>> ListFuncionariosByObraIdAsync(
        Guid obraId,
        CancellationToken cancellationToken = default);
}
