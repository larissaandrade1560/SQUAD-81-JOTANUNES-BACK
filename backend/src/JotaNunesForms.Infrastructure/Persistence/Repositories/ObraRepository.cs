using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using JotaNunesForms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class ObraRepository : IObraRepository
{
    private readonly JotaNunesFormsDbContext _dbContext;

    public ObraRepository(JotaNunesFormsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Obra?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Obras.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Obra>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Obras
            .AsNoTracking()
            .OrderBy(o => o.Nome)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsCodigoAsync(
        string codigo,
        Guid? excludeObraId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Obras.AsNoTracking().Where(o => o.Codigo == codigo);
        if (excludeObraId is not null)
        {
            query = query.Where(o => o.Id != excludeObraId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Obra obra, CancellationToken cancellationToken = default)
    {
        await _dbContext.Obras.AddAsync(obra, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Obra obra, CancellationToken cancellationToken = default)
    {
        _dbContext.Obras.Update(obra);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
