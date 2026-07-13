using StackExchange.Redis;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client.Abstractions;

public interface IDistributedCacheExecutor
{
    Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> action, CancellationToken ct);
    Task ExecuteAsync(Func<IDatabase, Task> action, CancellationToken ct);
}
