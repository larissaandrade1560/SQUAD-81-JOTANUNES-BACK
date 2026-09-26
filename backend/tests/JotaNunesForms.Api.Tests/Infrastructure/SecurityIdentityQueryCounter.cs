using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JotaNunesForms.Api.Tests.Infrastructure;

public sealed class SecurityIdentityQueryCounter : DbCommandInterceptor
{
    private int _count;

    public int Count => Volatile.Read(ref _count);

    public void Reset() => Interlocked.Exchange(ref _count, 0);

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        CountIfIdentityQuery(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        CountIfIdentityQuery(command);
        return ValueTask.FromResult(result);
    }

    private void CountIfIdentityQuery(DbCommand command)
    {
        if (command.CommandText.Contains("security_identity", StringComparison.OrdinalIgnoreCase))
        {
            Interlocked.Increment(ref _count);
        }
    }
}
