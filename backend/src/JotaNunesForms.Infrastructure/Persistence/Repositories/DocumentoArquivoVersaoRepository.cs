using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class DocumentoArquivoVersaoRepository : IDocumentoArquivoVersaoRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public DocumentoArquivoVersaoRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<DocumentoArquivoVersao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.DocumentosArquivosVersoes.FirstOrDefaultAsync(versao => versao.Id == id, cancellationToken);

    public Task<DocumentoArquivoVersao?> GetCurrentForEmpresaAsync(
        Guid documentoEmpresaId,
        CancellationToken cancellationToken = default) =>
        _db.DocumentosArquivosVersoes.FirstOrDefaultAsync(
            versao => versao.DocumentoEmpresaId == documentoEmpresaId && versao.Vigente,
            cancellationToken);

    public Task<DocumentoArquivoVersao?> GetCurrentForFuncionarioAsync(
        Guid documentoFuncionarioId,
        CancellationToken cancellationToken = default) =>
        _db.DocumentosArquivosVersoes.FirstOrDefaultAsync(
            versao => versao.DocumentoFuncionarioId == documentoFuncionarioId && versao.Vigente,
            cancellationToken);

    public async Task<int> GetNextNumberForEmpresaAsync(Guid documentoEmpresaId, CancellationToken cancellationToken = default) =>
        (await _db.DocumentosArquivosVersoes
            .Where(versao => versao.DocumentoEmpresaId == documentoEmpresaId)
            .Select(versao => (int?)versao.Numero)
            .MaxAsync(cancellationToken) ?? 0) + 1;

    public async Task<int> GetNextNumberForFuncionarioAsync(Guid documentoFuncionarioId, CancellationToken cancellationToken = default) =>
        (await _db.DocumentosArquivosVersoes
            .Where(versao => versao.DocumentoFuncionarioId == documentoFuncionarioId)
            .Select(versao => (int?)versao.Numero)
            .MaxAsync(cancellationToken) ?? 0) + 1;

    public async Task AddAsync(DocumentoArquivoVersao versao, CancellationToken cancellationToken = default)
    {
        await _db.DocumentosArquivosVersoes.AddAsync(versao, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DocumentoArquivoVersao versao, CancellationToken cancellationToken = default)
    {
        _db.DocumentosArquivosVersoes.Update(versao);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
