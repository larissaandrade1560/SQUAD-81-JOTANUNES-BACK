using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IDocumentoEmpresaRepository
{
    Task<DocumentoEmpresa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoEmpresa>> ListAsync(
        Guid? empresaId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoEmpresa>> ListPendentesAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(DocumentoEmpresa documento, CancellationToken cancellationToken = default);

    Task UpdateAsync(DocumentoEmpresa documento, CancellationToken cancellationToken = default);
}
