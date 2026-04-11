using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client;

public class DistributedCacheExecutor(
    IDistributedCacheClientFactory factory, 
    IOptions<DistributedCacheRetryOptions> options,
    ILogger<DistributedCacheExecutor> logger)
    : IDistributedCacheExecutor
{
    public async Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> action, CancellationToken ct = default)
    {
        int attempt = 0;
        TimeSpan delay = options.Value.InitialDelay;

        while (true)
        {
            try
            {
                return await action(factory.GetDatabase());
            }
            catch (RedisConnectionException ex) when (attempt < options.Value.MaxRetries)
            {
                if (attempt == 0)
                {
                    logger.LogWarning(ex, "Transient DistributedCache connection failure on attempt. Retrying...");    
                }
                delay = Backoff.DecorrelatedJitter(delay, options.Value.InitialDelay, options.Value.MaxDelay);
                await Task.Delay(delay, ct);
                attempt++;
            }
            catch (RedisTimeoutException ex) when (attempt < options.Value.MaxRetries)
            {
                if (attempt == 0)
                {
                    logger.LogWarning(ex, "Timeout DistributedCache connection failure on attempt. Retrying...");    
                }
                delay = Backoff.DecorrelatedJitter(delay, options.Value.InitialDelay, options.Value.MaxDelay);
                await Task.Delay(delay, ct);
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
}
