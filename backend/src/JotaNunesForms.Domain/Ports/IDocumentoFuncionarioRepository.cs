using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IDocumentoFuncionarioRepository
{
    Task<DocumentoFuncionario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoFuncionario>> ListByFuncionarioAsync(
        Guid funcionarioId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoFuncionario>> ListPendentesAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(DocumentoFuncionario documento, CancellationToken cancellationToken = default);

    Task UpdateAsync(DocumentoFuncionario documento, CancellationToken cancellationToken = default);
}
