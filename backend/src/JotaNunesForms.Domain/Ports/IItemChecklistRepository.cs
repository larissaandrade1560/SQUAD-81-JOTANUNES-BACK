using JotaNunesForms.Domain.Entities;

namespace JotaNunesForms.Domain.Ports;

public interface IItemChecklistRepository
{
    Task<ItemChecklist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItemChecklist>> ListByProcessoAsync(Guid processoId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<ItemChecklist> itens, CancellationToken cancellationToken = default);

    Task UpdateAsync(ItemChecklist item, CancellationToken cancellationToken = default);
}
