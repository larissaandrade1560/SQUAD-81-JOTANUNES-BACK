using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IDocumentoVersaoRepository
{
    Task<DocumentoVersao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoVersao>> ListByItemAsync(Guid itemChecklistId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentoVersao>> ListVigentesPendentesAsync(CancellationToken cancellationToken = default);

    Task AddAsync(DocumentoVersao versao, CancellationToken cancellationToken = default);

    Task UpdateAsync(DocumentoVersao versao, CancellationToken cancellationToken = default);

    Task AddAnaliseAsync(AnaliseDocumento analise, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnaliseDocumento>> ListAnalisesByVersaoAsync(
        Guid documentoVersaoId,
        CancellationToken cancellationToken = default);
}
