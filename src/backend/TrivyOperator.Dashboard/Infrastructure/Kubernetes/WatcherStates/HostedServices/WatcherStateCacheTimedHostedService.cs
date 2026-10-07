using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.HostedServices;

public sealed class WatcherStateCacheTimedHostedService(
    ICache<WatcherId, WatcherStateInfo> cache,
    IEnumerable<IClusterScopedWatcherRegistry> clusterScopedWatchers,
    IEnumerable<INamespacedWatcherRegistry> namespacedWatchers,
    IOptions<WatchersOptions> options,
    ILogger<WatcherStateCacheTimedHostedService> logger
) : IHostedService, IDisposable
{
    private readonly TimeSpan timeFrame = TimeSpan.FromSeconds(options.Value.WatchTimeoutInSeconds * 1.1 + 60);

    private bool disposed;
    private Task? executingTask;
    private CancellationTokenSource? stoppingCts;
    private Timer? timer;

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Watcher State Cache Timed Hosted Service is starting.");

        stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timer = new Timer(Execute, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Watcher State Cache Timed Hosted Service is stopping.");

        timer?.Change(Timeout.Infinite, 0);

        if (executingTask?.IsCompleted ?? true)
        {
            logger.LogInformation("Watcher State Cache Timed Hosted Service stopped.");
            return;
        }

        try
        {
            await stoppingCts!.CancelAsync();
        }
        finally
        {
            await executingTask.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
        
        logger.LogInformation("Watcher State Cache Timed Hosted Service stopped.");
    }

    ~WatcherStateCacheTimedHostedService()
    {
        Dispose(false);
    }

    private void Execute(object? state)
    {
        if (executingTask == null || executingTask.IsCompleted)
        {
            executingTask = ExecuteAsync(stoppingCts?.Token ?? CancellationToken.None);
        }
        else
        {
            logger.LogInformation(
                "Watcher State Cache Timed Hosted Service is still running previous execution, skip for next cycle."
            );
        }
    }

    private async Task ExecuteAsync(CancellationToken ctx = default)
    {
        try
        {
            // get expired watchers
            WatcherStateInfo[] expiredWatcherStates =
            [
                .. cache.Select(kvp => kvp.Value)
                    .Where(x => DateTime.UtcNow - x.LastEventMoment > timeFrame)
            ];

            if (expiredWatcherStates.Length == 0)
                return;

            // get all registered watcher registries
            Dictionary<Type, IKubernetesWatcherRegistry> watcherRegistries = [];

            foreach (INamespacedWatcherRegistry watcher in namespacedWatchers)
            {
                watcherRegistries.TryAdd(watcher.WatchedKubernetesObjectType, watcher);
            }

            foreach (IClusterScopedWatcherRegistry watcher in clusterScopedWatchers)
            {
                watcherRegistries.TryAdd(watcher.WatchedKubernetesObjectType, watcher);
            }

            // for each expired watcher, recreate it
            foreach (WatcherStateInfo expiredWatcherState in expiredWatcherStates)
            {
                if (watcherRegistries.TryGetValue(
                        expiredWatcherState.Id.WatchedKubernetesObjectType,
                        out IKubernetesWatcherRegistry? watcher))
                {
                    await watcher.RecreateWatcher(expiredWatcherState.Id.Location, ctx);
                }

                ctx.ThrowIfCancellationRequested();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Watcher State Cache Timed Hosted Service execution has crashed.");
        }
    }


    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            timer?.Dispose();
            stoppingCts?.Cancel();
        }

        disposed = true;
    }
}
