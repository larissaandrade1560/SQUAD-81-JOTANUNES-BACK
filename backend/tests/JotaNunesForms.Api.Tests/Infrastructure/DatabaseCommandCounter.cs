using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JotaNunesForms.Api.Tests.Infrastructure;

public sealed class DatabaseCommandCounter : DbCommandInterceptor
{
    private int _readerCommands;

    public int ReaderCommands => Volatile.Read(ref _readerCommands);

    public void Reset() => Interlocked.Exchange(ref _readerCommands, 0);

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _readerCommands);
        return ValueTask.FromResult(result);
    }
}
