using StackExchange.Redis;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;

public interface IDistributedCacheExecutor
{
    Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> action);
    Task ExecuteAsync(Func<IDatabase, Task> action);
}
