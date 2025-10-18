using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Services.Alerts;
using TrivyOperator.Dashboard.Application.Services.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Utils;

namespace TrivyOperator.Dashboard.Application.Services.WatcherStateAlertRefreshers;

public class WatcherStateAlertRefresh<TKubernetesObject>(
    IAlertsService alertService,
    ILogger<WatcherStateAlertRefresh<TKubernetesObject>> logger)
    : IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    private const string AlertEmitter = "Watcher";
    private static readonly HashSet<string> ActiveAlerts = [];
    
    public async Task ProcessKubernetesEvent(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken cancellationToken)
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
                await RemoveAlert(watcherEvent, cancellationToken);
                break;
            case WatcherEventType.Error:
                await AddAlert(watcherEvent, cancellationToken);
                break;
            case WatcherEventType.Unknown:
                logger.LogWarning("{watcherEventType} event type {eventType} for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType.ToString(), watcherEvent.WatcherEventType, typeof(TKubernetesObject).Name);
                break;
            default:
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

        string namespaceName = watcherEvent.WatcherKey == CacheUtils.DefaultCacheRefreshKey ? "n/a" : watcherEvent.WatcherKey;
        await alertService.AddAlert(
            AlertEmitter,
            new Alert
            {
                EmitterKey = GetCacheKey(watcherEvent),
                Message = $"Watcher for {typeof(TKubernetesObject).Name} and Namespace {namespaceName} failed.",
                Severity = Severity.Error,
                Category = "Watcher Failed",
            }, cancellationToken);
    }

    private async ValueTask RemoveAlert(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken cancellationToken)
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
            cancellationToken);
        }
    }

    private static string GetCacheKey(IWatcherEvent<TKubernetesObject> watcherEvent) =>
        $"{typeof(TKubernetesObject).Name}|{watcherEvent.WatcherKey}";
}
