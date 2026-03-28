using StackExchange.Redis;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;

public interface IDistributedCacheClientFactory
{
    IDatabase GetDatabase();
    ISubscriber GetSubscriber();
    void Dispose();
}
