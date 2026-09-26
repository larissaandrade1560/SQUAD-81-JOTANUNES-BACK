using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class DocumentoVersaoRepository : IDocumentoVersaoRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public DocumentoVersaoRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<DocumentoVersao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.DocumentosVersoes.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DocumentoVersao>> ListByItemAsync(
        Guid itemChecklistId,
        CancellationToken cancellationToken = default) =>
        await _db.DocumentosVersoes
            .Where(v => v.ItemChecklistId == itemChecklistId)
            .OrderByDescending(v => v.Numero)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DocumentoVersao>> ListVigentesPendentesAsync(
        CancellationToken cancellationToken = default) =>
        await (
            from versao in _db.DocumentosVersoes
            join item in _db.ItensChecklist on versao.ItemChecklistId equals item.Id
            where versao.Vigente
                  && item.Ativo
                  && item.Situacao == SituacaoItemChecklist.PendenteAnalise
            select versao)
            .OrderBy(v => v.EnviadoEm)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(DocumentoVersao versao, CancellationToken cancellationToken = default)
    {
        await _db.DocumentosVersoes.AddAsync(versao, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DocumentoVersao versao, CancellationToken cancellationToken = default)
    {
        _db.DocumentosVersoes.Update(versao);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAnaliseAsync(AnaliseDocumento analise, CancellationToken cancellationToken = default)
    {
        await _db.AnalisesDocumento.AddAsync(analise, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AnaliseDocumento>> ListAnalisesByVersaoAsync(
        Guid documentoVersaoId,
        CancellationToken cancellationToken = default) =>
        await _db.AnalisesDocumento
            .Where(a => a.DocumentoVersaoId == documentoVersaoId)
            .OrderByDescending(a => a.AnalisadoEm)
            .ToListAsync(cancellationToken);
}
