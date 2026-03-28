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
    public async Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> action, CancellationToken ct = default)
    {
        int attempt = 0;
        TimeSpan delay = options.InitialDelay;

        while (true)
        {
            try
            {
                return await action(factory.GetDatabase());
            }
            catch (RedisConnectionException ex) when (attempt < options.MaxRetries)
            {
                if (attempt == 0)
                {
                    logger.LogWarning(ex, "Transient DistributedCache connection failure on attempt {ConnectionAttempt}. Retrying...", attempt + 1);    
                }
                await Task.Delay(ApplyJitter(delay), ct);
                delay = Backoff(delay, options.MaxDelay);
                attempt++;
            }
            catch (RedisTimeoutException ex) when (attempt < options.MaxRetries)
            {
                if (attempt == 0)
                {
                    logger.LogWarning(ex, "Transient DistributedCache connection failure on attempt {ConnectionAttempt}. Retrying...", attempt + 1);    
                }
                await Task.Delay(ApplyJitter(delay), ct);
                delay = Backoff(delay, options.MaxDelay);
                attempt++;
            }
        }
    }

    public async Task ExecuteAsync(Func<IDatabase, Task> action, CancellationToken ct = default)
    {
        await ExecuteAsync<object>(async db =>
        {
            await action(db);
            return null!;
        }, ct);
    }

    private static TimeSpan Backoff(TimeSpan current, TimeSpan max)
    {
        TimeSpan next = TimeSpan.FromMilliseconds(
            Math.Min(max.TotalMilliseconds,
                Random.Shared.NextDouble() * current.TotalMilliseconds * 3));

        return next;
    }

    private static TimeSpan ApplyJitter(TimeSpan delay)
    {
        double jitter = 0.2 + Random.Shared.NextDouble() * 0.6;
        return TimeSpan.FromMilliseconds(delay.TotalMilliseconds * jitter);
    }
}
