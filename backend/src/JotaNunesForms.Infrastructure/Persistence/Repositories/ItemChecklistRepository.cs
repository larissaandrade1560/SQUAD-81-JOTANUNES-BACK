using JotaNunesForms.Domain.Entities;
using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence.Repositories;

public sealed class ItemChecklistRepository : IItemChecklistRepository
{
    private readonly JotaNunesFormsDbContext _db;

    public ItemChecklistRepository(JotaNunesFormsDbContext db) => _db = db;

    public Task<ItemChecklist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.ItensChecklist.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ItemChecklist>> ListByProcessoAsync(
        Guid processoId,
        CancellationToken cancellationToken = default) =>
        await _db.ItensChecklist
            .Where(i => i.ProcessoId == processoId)
            .ToListAsync(cancellationToken);

    public async Task AddRangeAsync(IEnumerable<ItemChecklist> itens, CancellationToken cancellationToken = default)
    {
        await _db.ItensChecklist.AddRangeAsync(itens, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ItemChecklist item, CancellationToken cancellationToken = default)
    {
        _db.ItensChecklist.Update(item);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
