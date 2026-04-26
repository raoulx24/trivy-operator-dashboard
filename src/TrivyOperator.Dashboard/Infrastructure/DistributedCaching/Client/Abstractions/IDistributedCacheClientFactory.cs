using StackExchange.Redis;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCaching.Client.Abstractions;

public interface IDistributedCacheClientFactory
{
    Task<IDatabase> GetDatabase(CancellationToken ct = default);
    Task<ISubscriber> GetSubscriber(CancellationToken ct = default);
}
