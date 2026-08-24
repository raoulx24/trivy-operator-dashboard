using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;

namespace TrivyOperator.Dashboard.Application.Alerts.Refresher;

public class WatcherStateAlertRefresh<TKubernetesObject>(
    IAlertPublisher alertPublisher,
    ILogger<WatcherStateAlertRefresh<TKubernetesObject>> logger
) : IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new()
{
    private const string AlertEmitter = "Watcher";
    private static readonly HashSet<WatcherKey> ActiveAlerts = [];

    public async Task ProcessKubernetesEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx = default)
    {
        if (watcherEvent.IsStatic)
        {
            return;
        }

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
                    typeof(TKubernetesObject).Name
                );
                break;
        }
    }

    private async ValueTask AddAlert(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx = default)
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
                Message = $"Watcher for {typeof(TKubernetesObject).Name}, context {watcherEvent.Key.ContextName} and {watcherEvent.Key.NamespaceName} failed.",
                Severity = Severity.Error,
                Category = "Watcher Failed",
            },
            ctx
        );
    }

    private async ValueTask RemoveAlert(
        WatcherEvent<TKubernetesObject> watcherEvent,
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
    private static EmitterKey GetCacheKey(WatcherEvent<TKubernetesObject> watcherEvent) =>
        new([typeof(TKubernetesObject).Name, watcherEvent.Key.ContextName.Value, watcherEvent.Key.NamespaceName.Value,]);
}
