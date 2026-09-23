using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Options;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.HostedServices;

public sealed class WatcherStateCacheTimedHostedService(
    ICache<ResourceLocation, WatcherStateInfo> cache,
    IEnumerable<IClusterScopedWatcherRegistry> clusterScopedWatchers,
    IEnumerable<INamespacedWatcherRegistry> namespacedWatchers,
    IOptions<WatchersOptions> options,
    ILogger<WatcherStateCacheTimedHostedService> logger
) : IHostedService, IDisposable
{
    private readonly int timeFrameInSeconds = (int)((options.Value.WatchTimeoutInSeconds * 1.1) + 60);
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
            WatcherStateInfo[] expiredWatcherStates = [.. cache.Select(kvp => kvp.Value)
                .Where(x => (DateTime.UtcNow - x.LastEventMoment).TotalSeconds > timeFrameInSeconds),];
            
            if (expiredWatcherStates.Length == 0)
                return;

            Dictionary<Type, IKubernetesWatcherRegistry> watchers = [];
            
            foreach (INamespacedWatcherRegistry watcher in namespacedWatchers)
            {
                watchers.TryAdd(watcher.WatchedKubernetesObjectType, watcher);
            }

            foreach (IClusterScopedWatcherRegistry watcher in clusterScopedWatchers)
            {
                watchers.TryAdd(watcher.WatchedKubernetesObjectType, watcher);
            }

            foreach (WatcherStateInfo expiredWatcherState in expiredWatcherStates)
            {
                watchers.TryGetValue(
                    expiredWatcherState.WatchedKubernetesObjectType,
                    out IKubernetesWatcherRegistry? watcher);
                
                if (watcher is not null)
                {
                    await watcher.RecreateWatcher(expiredWatcherState.Key, ctx);
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
