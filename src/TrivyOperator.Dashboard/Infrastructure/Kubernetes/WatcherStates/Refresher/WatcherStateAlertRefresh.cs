using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventProcessors.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Refresher;

public class WatcherStateAlertRefresh<TResource, TKey>(
    IAlertPublisher alertPublisher,
    ILogger<WatcherStateAlertRefresh<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    private const string AlertEmitter = "Watcher";
    private static readonly HashSet<ResourceLocation> ActiveAlerts = [];

    public async Task ProcessKubernetesEvent(WatcherEvent<TResource, TKey> watcherEvent, CancellationToken ctx = default)
    {
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
            case WatcherEventType.Deleted:
            case WatcherEventType.Modified:
            case WatcherEventType.Bookmark:
            case WatcherEventType.WatcherConnected:
            case WatcherEventType.Flushed:
            case WatcherEventType.Initialized:
                await RemoveAlert(watcherEvent, ctx);
                break;
            case WatcherEventType.Error:
                await AddAlert(watcherEvent, ctx);
                break;
            case WatcherEventType.Unknown:
                logger.LogWarning(
                    "{watcherEventType} event type {eventType} for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType.ToString(),
                    watcherEvent.WatcherEventType,
                    typeof(TResource).Name
                );
                break;
        }
    }

    private async ValueTask AddAlert(WatcherEvent<TResource, TKey> watcherEvent, CancellationToken ctx = default)
    {
        if (ActiveAlerts.Contains(watcherEvent.Key))
        {
            return;
        }

        ActiveAlerts.Add(watcherEvent.Key);

        await alertPublisher.AddAlert(
            AlertEmitter,
            new Alert
            {
                Key = GetCacheKey(watcherEvent),
                Message = $"Watcher for {typeof(TResource).Name}, context {watcherEvent.Key.ContextName} and {watcherEvent.Key.NamespaceName} failed.",
                Severity = Severity.Error,
                Category = "Watcher Failed",
            },
            ctx
        );
    }

    private async ValueTask RemoveAlert(
        WatcherEvent<TResource, TKey> watcherEvent,
        CancellationToken cancellationToken
    )
    {
        if (ActiveAlerts.Contains(watcherEvent.Key))
        {
            ActiveAlerts.Remove(watcherEvent.Key);

            await alertPublisher.RemoveAlert(
                AlertEmitter,
                new Alert
                {
                    Key = GetCacheKey(watcherEvent),
                },
                cancellationToken
            );
        }
    }
    
    // TODO: change this to IReadOnlyList<string> and create a IEqual for it
    private static EmitterKey GetCacheKey(WatcherEvent<TResource, TKey> watcherEvent) =>
        new([typeof(TResource).Name, watcherEvent.Key.ContextName.Value, watcherEvent.Key.NamespaceName.Value,]);
}
