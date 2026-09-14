using StackExchange.Redis;

namespace TrivyOperator.Dashboard.Composition.History;

public static class HistoryStartupChecks
{
    public static ILogger? Logger { get; set; }
    
    public static async Task CheckDistributedCacheConnectivity(
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        HistoryCompositionMode mode = HistoryCompositionResolver.Resolve(configuration);

        if (mode != HistoryCompositionMode.DistributedCache)
            return;

        string connString = configuration.GetValue<string?>("History:DistributedCache:ConnectionString") ??
                            throw new InvalidOperationException("Distributed Cache connection string missing.");

        TimeSpan timeout = TimeSpan.FromSeconds(60);
        TimeSpan delay = TimeSpan.FromSeconds(1);

        using CancellationTokenSource overallCts = new(timeout);

        while (!overallCts.Token.IsCancellationRequested)
        {
            try
            {
                using CancellationTokenSource connectCts =
                    CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
                connectCts.CancelAfter(TimeSpan.FromSeconds(5)); // per-attempt timeout

                ConnectionMultiplexer conn = await ConnectionMultiplexer.ConnectAsync(connString);
                await conn.GetDatabase().PingAsync();

                await conn.DisposeAsync();

                Logger?.LogInformation("Distributed Cache connectivity check succeeded.");
                return;
            }
            catch (OperationCanceledException) when (overallCts.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(ex, "Distributed Cache (Redis/Valkey) is not reachable, retrying in {Delay}", delay);

                try
                {
                    await Task.Delay(delay, overallCts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                // simple backoff
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 5));
            }
        }

        throw new InvalidOperationException("Distributed Cache server is not reachable after retries.");
    }
}
