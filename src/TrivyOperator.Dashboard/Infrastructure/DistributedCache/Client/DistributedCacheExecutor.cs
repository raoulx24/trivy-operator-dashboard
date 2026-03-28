using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client;

public class DistributedCacheExecutor(
    IDistributedCacheClientFactory factory, 
    DistributedCacheRetryOptions options,
    ILogger<DistributedCacheExecutor> logger)
    : IDistributedCacheExecutor
{
    private IDatabase db => factory.GetDatabase();
    private static readonly Random rng = new();

    public async Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> action)
    {
        int attempt = 0;
        TimeSpan delay = options.InitialDelay;

        while (true)
        {
            try
            {
                return await action(db);
            }
            catch (RedisConnectionException ex) when (attempt < options.MaxRetries)
            {
                logger.LogWarning(ex, "Transient DistributedCache connection failure on attempt {ConnectionAttempt}. Retrying...", attempt + 1);
                await Task.Delay(ApplyJitter(delay));
                delay = Cap(Backoff(delay), options.MaxDelay);
                attempt++;
            }
            catch (RedisException ex) when (attempt < options.MaxRetries)
            {
                logger.LogWarning(ex, "Transient DistributedCache failure on attempt {ConnectionAttempt}. Retrying...", attempt + 1);
                await Task.Delay(ApplyJitter(delay));
                delay = Cap(Backoff(delay), options.MaxDelay);
                attempt++;
            }
        }
    }

    public async Task ExecuteAsync(Func<IDatabase, Task> action)
    {
        await ExecuteAsync<object>(async db =>
        {
            await action(db);
            return null!;
        });
    }

    private static TimeSpan Backoff(TimeSpan current)
        => TimeSpan.FromMilliseconds(current.TotalMilliseconds * 2);

    private static TimeSpan Cap(TimeSpan current, TimeSpan maxDelay)
        => current > maxDelay ? maxDelay : current;

    private static TimeSpan ApplyJitter(TimeSpan delay)
    {
        double jitter = 0.2 + rng.NextDouble() * 0.6;
        return TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitter);
    }
}
