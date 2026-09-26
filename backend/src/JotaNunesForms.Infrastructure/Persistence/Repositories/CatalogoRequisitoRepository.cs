using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class CatalogoRequisitoRepository : ICatalogoRequisitoRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public CatalogoRequisitoRepository(JotaNunesFormsDbContext db) => _db = db;

    public async Task<IReadOnlyList<CatalogoRequisito>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.CatalogoRequisitos.AsNoTracking().OrderBy(r => r.Codigo).ToListAsync(cancellationToken);

    public Task<CatalogoRequisito?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.CatalogoRequisitos.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<CatalogoRequisito?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default) =>
        _db.CatalogoRequisitos.FirstOrDefaultAsync(r => r.Codigo == codigo, cancellationToken);

    public async Task AddAsync(CatalogoRequisito requisito, CancellationToken cancellationToken = default)
    {
        await _db.CatalogoRequisitos.AddAsync(requisito, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CatalogoRequisito requisito, CancellationToken cancellationToken = default)
    {
        _db.CatalogoRequisitos.Update(requisito);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParametroNormativo>> ListParametrosAsync(CancellationToken cancellationToken = default) =>
        await _db.ParametrosNormativos.AsNoTracking().OrderBy(p => p.Chave).ToListAsync(cancellationToken);

    public Task<ParametroNormativo?> GetParametroByChaveAsync(string chave, CancellationToken cancellationToken = default) =>
        _db.ParametrosNormativos.FirstOrDefaultAsync(p => p.Chave == chave, cancellationToken);

    public async Task AddParametroAsync(ParametroNormativo parametro, CancellationToken cancellationToken = default)
    {
        await _db.ParametrosNormativos.AddAsync(parametro, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateParametroAsync(ParametroNormativo parametro, CancellationToken cancellationToken = default)
    {
        _db.ParametrosNormativos.Update(parametro);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EscopoArt>> ListEscoposArtAsync(CancellationToken cancellationToken = default) =>
        await _db.EscoposArt.AsNoTracking().OrderBy(e => e.Nome).ToListAsync(cancellationToken);
}
