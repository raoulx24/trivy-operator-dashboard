using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client;

public class DistributedCacheClientFactory(string connectionString, ILogger<DistributedCacheClientFactory> logger)
    : IDistributedCacheClientFactory, IDisposable
{
    private ConnectionMultiplexer? connection;
    private readonly object localMutex = new();

    private ConnectionMultiplexer GetOrCreate()
    {
        if (connection is { IsConnected: true })
            return connection;

        logger.LogDebug("Trying to (re)create connection6");
        lock (localMutex)
        {
            if (connection is { IsConnected: true })
                return connection;

            connection?.Dispose();
            connection = ConnectionMultiplexer.Connect(connectionString);
            return connection;
        }
    }

    public IDatabase GetDatabase() => GetOrCreate().GetDatabase();

    public ISubscriber GetSubscriber() => GetOrCreate().GetSubscriber();

    public void Dispose()
    {
        connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}