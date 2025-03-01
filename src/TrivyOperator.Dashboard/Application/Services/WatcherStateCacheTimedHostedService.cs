﻿using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Services.Options;
using TrivyOperator.Dashboard.Application.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherStates;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services;

public class WatcherStateCacheTimedHostedService(
    IConcurrentCache<string, WatcherStateInfo> cache,
    IServiceProvider serviceProvider,
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

        if (executingTask == null || executingTask.IsCompleted)
        {
            return;
        }

        try
        {
            stoppingCts!.Cancel();
        }
        finally
        {
            await executingTask.WaitAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
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
                "Watcher State Cache Timed Hosted Service is still running previous execution, wait for next cycle."
            );
        }
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<WatcherStateInfo> expiredWatcherStates = cache.Select(kvp => kvp.Value)
                .Where(x => (DateTime.Now - x.LastEventMoment).TotalSeconds > timeFrameInSeconds);

            foreach (WatcherStateInfo expiredWatcherState in expiredWatcherStates)
            {
                Type clusteredScopedWatcherType =
                    typeof(IClusterScopedWatcher<>).MakeGenericType(expiredWatcherState.WatchedKubernetesObjectType);
                Type namespacedWatcherType =
                    typeof(INamespacedWatcher<>).MakeGenericType(expiredWatcherState.WatchedKubernetesObjectType);

                object? watcherService = serviceProvider.GetServices(clusteredScopedWatcherType).FirstOrDefault() ??
                                         serviceProvider.GetServices(namespacedWatcherType).FirstOrDefault();

                if (watcherService is IKubernetesWatcher watcher)
                {
                    await watcher.Recreate(cancellationToken, expiredWatcherState.WatcherKey);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Watcher State Cache Timed Hosted Service execution has crashed.");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                timer?.Dispose();
                stoppingCts?.Cancel();
            }

            disposed = true;
        }
    }
}
