using StackExchange.Redis;
using TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;

public interface IDistributedCacheClientFactory
{
    IDatabase GetDatabase();
    ISubscriber GetSubscriber();
    void Dispose();
}
