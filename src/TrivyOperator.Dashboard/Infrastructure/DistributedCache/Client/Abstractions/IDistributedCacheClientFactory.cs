using StackExchange.Redis;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;

public interface IDistributedCacheClientFactory
{
    Task<IDatabase> GetDatabase(CancellationToken ct = default);
    Task<ISubscriber> GetSubscriber(CancellationToken ct = default);
}
