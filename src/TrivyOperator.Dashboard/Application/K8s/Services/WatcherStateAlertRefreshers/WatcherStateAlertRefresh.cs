using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Alerts.Services;
using TrivyOperator.Dashboard.Application.Alerts.Services.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherStateAlertRefreshers;

public class WatcherStateAlertRefresh<TKubernetesObject>(
    IAlertsService alertService,
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

        await alertService.AddAlert(
            AlertEmitter,
            new Alert
            {
                EmitterKey = GetCacheKey(watcherEvent),
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

            await alertService.RemoveAlert(
                AlertEmitter,
                new Alert
                {
                    EmitterKey = GetCacheKey(watcherEvent),
                },
                cancellationToken
            );
        }
    }

    private static string GetCacheKey(WatcherEvent<TKubernetesObject> watcherEvent) =>
        $"{typeof(TKubernetesObject).Name}|{watcherEvent.Key.ContextName}|{watcherEvent.Key.NamespaceName}";
}
