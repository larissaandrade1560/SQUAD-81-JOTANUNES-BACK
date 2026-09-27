using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IDocumentoArquivoVersaoRepository
{
    Task<DocumentoArquivoVersao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DocumentoArquivoVersao?> GetCurrentForEmpresaAsync(Guid documentoEmpresaId, CancellationToken cancellationToken = default);

    Task<DocumentoArquivoVersao?> GetCurrentForFuncionarioAsync(Guid documentoFuncionarioId, CancellationToken cancellationToken = default);

    Task<int> GetNextNumberForEmpresaAsync(Guid documentoEmpresaId, CancellationToken cancellationToken = default);

    Task<int> GetNextNumberForFuncionarioAsync(Guid documentoFuncionarioId, CancellationToken cancellationToken = default);

    Task AddAsync(DocumentoArquivoVersao versao, CancellationToken cancellationToken = default);

    Task UpdateAsync(DocumentoArquivoVersao versao, CancellationToken cancellationToken = default);
}
