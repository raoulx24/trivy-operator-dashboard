using StackExchange.Redis;
using TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.Distributed.Client;

public sealed class DistributedCacheClientFactory(DistributedCacheConnectionProvider provider) : IDistributedCacheClientFactory
{
    public async Task<IDatabase> GetDatabase(CancellationToken ct = default)
    {
        ConnectionMultiplexer conn = await provider.GetAsync(ct);
        return conn.GetDatabase();
    }

    public async Task<ISubscriber> GetSubscriber(CancellationToken ct = default)
    {
        ConnectionMultiplexer conn = await provider.GetAsync(ct);
        return conn.GetSubscriber();
    }
}
