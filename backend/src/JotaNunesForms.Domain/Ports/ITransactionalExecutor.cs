namespace JotaNunesForms.Domain.Ports;

public interface ITransactionalExecutor
{
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
