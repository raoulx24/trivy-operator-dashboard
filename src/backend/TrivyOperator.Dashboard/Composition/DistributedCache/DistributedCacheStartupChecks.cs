using StackExchange.Redis;
using TrivyOperator.Dashboard.Composition.Shared;

namespace TrivyOperator.Dashboard.Composition.DistributedCache;

public static class DistributedCacheStartupChecks
{
    public static async Task CheckDistributedCacheConnectivity(
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (!DistributedCacheCompositionResolver.IsRequired(configuration))
            return;

        string connString = configuration.GetValue<string?>("History:DistributedCache:ConnectionString") ??
                            throw new InvalidOperationException("Distributed Cache connection string missing.");

        TimeSpan timeout = TimeSpan.FromSeconds(60);
        TimeSpan delay = TimeSpan.FromSeconds(1);

        using CancellationTokenSource overallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        overallCts.CancelAfter(timeout);

        while (!overallCts.Token.IsCancellationRequested)
        {
            // every attempt's multiplexer is disposed: with abortConnect=False, ConnectAsync returns one even when the
            // server is down, and it keeps reconnecting in the background until disposed
            ConnectionMultiplexer? conn = null;

            try
            {
                using CancellationTokenSource connectCts =
                    CancellationTokenSource.CreateLinkedTokenSource(overallCts.Token);
                connectCts.CancelAfter(TimeSpan.FromSeconds(5)); // per-attempt timeout

                // the client takes no token: the attempt stops waiting at the timeout, and a multiplexer that arrives
                // after that is disposed when it does
                Task<ConnectionMultiplexer> connectTask = ConnectionMultiplexer.ConnectAsync(connString);

                try
                {
                    conn = await connectTask.WaitAsync(connectCts.Token);
                }
                catch (OperationCanceledException)
                {
                    _ = connectTask.ContinueWith(
                        static t => t.Result.Dispose(),
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.Default);
                    throw;
                }

                await conn.GetDatabase().PingAsync().WaitAsync(connectCts.Token);

                CompositionLogger.Logger?.LogInformation("Distributed Cache connectivity check succeeded.");
                return;
            }
            catch (OperationCanceledException) when (overallCts.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                CompositionLogger.Logger?.LogWarning(ex, "Distributed Cache (Redis/Valkey) is not reachable, retrying in {Delay}", delay);

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
            finally
            {
                if (conn is not null)
                {
                    await conn.DisposeAsync();
                }
            }
        }

        throw new InvalidOperationException("Distributed Cache server is not reachable after retries.");
    }
}
