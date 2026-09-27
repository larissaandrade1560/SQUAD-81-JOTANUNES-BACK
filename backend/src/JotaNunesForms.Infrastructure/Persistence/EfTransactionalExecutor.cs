using JotaNunesForms.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace JotaNunesForms.Infrastructure.Persistence;

public sealed class EfTransactionalExecutor : ITransactionalExecutor
{
    private readonly JotaNunesFormsDbContext _db;

    public EfTransactionalExecutor(JotaNunesFormsDbContext db) => _db = db;

    public Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        if (_db.Database.CurrentTransaction is not null)
        {
            return operation(cancellationToken);
        }

        var strategy = _db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _db.ChangeTracker.Clear();
                throw;
            }
        });
    }
}
