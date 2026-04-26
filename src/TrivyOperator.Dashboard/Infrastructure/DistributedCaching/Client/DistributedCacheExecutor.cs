using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TrivyOperator.Dashboard.Infrastructure.DistributedCaching.Client.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCaching.Client;

public class DistributedCacheExecutor(
    IDistributedCacheClientFactory factory, 
    IOptions<DistributedCacheRetryOptions> options,
    ILogger<DistributedCacheExecutor> logger)
    : IDistributedCacheExecutor
{
    // WARNING: This operation may be retried on transient Redis failures.
    // The provided delegate MUST be idempotent and safe to execute multiple times
    // without causing inconsistent state or duplicate side effects
    // Example:
    // var value = await executor.ExecuteAsync(db => db.StringGetAsync(key));
    // var updated = Transform(value);
    // await executor.ExecuteAsync(db => db.StringSetAsync(key, updated));
    public async Task<T> ExecuteAsync<T>(Func<IDatabase, Task<T>> action, CancellationToken ct = default)
    {
        int attempt = 0;
        TimeSpan delay = options.Value.InitialDelay;

        while (true)
        {
            try
            {
                IDatabase db = await factory.GetDatabase(ct);
                return await action(db);
            }
            catch (Exception ex) when (ex is RedisConnectionException or RedisTimeoutException &&
                                       attempt < options.Value.MaxRetries)
            {
                if (attempt == 0)
                {
                    logger.LogWarning(
                        ex,
                        "Distributed Cache failure (attempt {Attempt}/{MaxRetries}). Retrying in {Delay}...",
                        attempt + 1,
                        options.Value.MaxRetries,
                        delay
                    );
                }

                delay = Backoff.DecorrelatedJitter(delay, options.Value.InitialDelay, options.Value.MaxDelay);
                attempt++;
                await Task.Delay(delay, ct);
            }
            catch (Exception ex) when (ex is not RedisConnectionException and not RedisTimeoutException)
            {
                logger.LogError(ex, "Non-retryable Distributed Cache error.");
                throw;
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
