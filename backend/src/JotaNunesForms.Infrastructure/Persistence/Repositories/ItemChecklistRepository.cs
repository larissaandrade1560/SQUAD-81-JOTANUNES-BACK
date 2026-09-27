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

    public async Task<ItemChecklist?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tracked = _db.ChangeTracker.Entries<ItemChecklist>()
            .FirstOrDefault(entry => entry.Entity.Id == id);
        if (tracked is not null)
        {
            tracked.State = EntityState.Detached;
        }

        return await _db.ItensChecklist
            .FromSqlInterpolated($"SELECT * FROM itens_checklist WHERE id = {id} FOR UPDATE")
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ItemChecklist>> ListByProcessoAsync(
        Guid processoId,
        CancellationToken cancellationToken = default) =>
        await _db.ItensChecklist
            .Where(i => i.ProcessoId == processoId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ItemChecklist>> ListActiveWorkerItemsByMobilizacaoAsync(
        Guid mobilizacaoId,
        Guid processoId,
        CancellationToken cancellationToken = default) =>
        await _db.ItensChecklist
            .Where(i =>
                i.ProcessoId == processoId
                && i.TitularTipo == TitularRequisito.Trabalhador
                && i.TitularId == mobilizacaoId
                && i.Ativo)
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
