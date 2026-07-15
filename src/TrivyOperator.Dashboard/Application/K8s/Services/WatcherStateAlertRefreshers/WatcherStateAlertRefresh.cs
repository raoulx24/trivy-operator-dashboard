using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Alerts.Services;
using TrivyOperator.Dashboard.Application.Alerts.Services.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.WatcherStateAlertRefreshers;

public class WatcherStateAlertRefresh<TKubernetesObject>(
    IAlertsService alertService,
    ILogger<WatcherStateAlertRefresh<TKubernetesObject>> logger
) : IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    private const string AlertEmitter = "Watcher";
    private static readonly HashSet<string> ActiveAlerts = [];

    public async Task ProcessKubernetesEvent(
        IWatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken ctx
    )
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

    private async ValueTask AddAlert(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken cancellationToken)
    {
        if (ActiveAlerts.Contains(watcherEvent.WatcherKey))
        {
            return;
        }

        ActiveAlerts.Add(watcherEvent.WatcherKey);

        string namespaceName = watcherEvent.WatcherKey == CacheUtils.DefaultCacheRefreshKey ? "n/a"
            : watcherEvent.WatcherKey;
        await alertService.AddAlert(
            AlertEmitter,
            new Alert
            {
                EmitterKey = GetCacheKey(watcherEvent),
                Message = $"Watcher for {typeof(TKubernetesObject).Name} and Namespace {namespaceName} failed.",
                Severity = Severity.Error,
                Category = "Watcher Failed",
            },
            cancellationToken
        );
    }

    private async ValueTask RemoveAlert(
        IWatcherEvent<TKubernetesObject> watcherEvent,
        CancellationToken cancellationToken
    )
    {
        if (ActiveAlerts.Contains(watcherEvent.WatcherKey))
        {
            ActiveAlerts.Remove(watcherEvent.WatcherKey);

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

    private static string GetCacheKey(IWatcherEvent<TKubernetesObject> watcherEvent) =>
        $"{typeof(TKubernetesObject).Name}|{watcherEvent.WatcherKey}";
}
