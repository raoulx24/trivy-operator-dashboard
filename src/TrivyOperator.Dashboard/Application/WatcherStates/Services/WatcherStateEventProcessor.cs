using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.WatcherStates.Models;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.WatcherStates.Services;

public class WatcherStateEventProcessor<TKubernetesObject>(
    IConcurrentCache<WatcherKey, WatcherStateInfo> cache,
    ILogger<WatcherStateEventProcessor<TKubernetesObject>> logger
) : IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, new()
{
    private readonly DictionaryCounter eventsGauge = new();

    public Task ProcessKubernetesEvent(WatcherEvent<TKubernetesObject> watcherEvent, CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
        if (watcherEvent.IsStatic)
        {
            return Task.CompletedTask;
        }

        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
                eventsGauge.OffsetValue(watcherEvent.Key, 1);
                ProcessGreenEvent(watcherEvent);
                break;
            case WatcherEventType.Deleted:
                eventsGauge.OffsetValue(watcherEvent.Key, -1);
                ProcessGreenEvent(watcherEvent);
                break;
            case WatcherEventType.Modified:
            case WatcherEventType.Bookmark:
            case WatcherEventType.WatcherConnected:
                ProcessGreenEvent(watcherEvent);
                break;
            case WatcherEventType.Flushed:
                eventsGauge.RemoveKey(watcherEvent.Key);
                ProcessFlushedEvent(watcherEvent);
                break;
            case WatcherEventType.Error:
                eventsGauge.RemoveKey(watcherEvent.Key);
                ProcessRedEvent(watcherEvent);
                break;
            case WatcherEventType.Initialized:
                break;
            case WatcherEventType.Unknown:
                logger.LogWarning(
                    "{watcherEventType} event type for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType.ToString(),
                    typeof(TKubernetesObject).Name
                );
                break;
        }

        return Task.CompletedTask;
    }

    private void ProcessGreenEvent(WatcherEvent<TKubernetesObject> watcherEvent)
    {
        WatcherStateInfo watcherStateInfo = new()
        {
            Key = watcherEvent.Key,
            WatchedKubernetesObjectType = typeof(TKubernetesObject),
            LastException = null,
            LastEventMoment = DateTime.UtcNow,
            Status = WatcherStateStatus.Green,
            EventsGauge = eventsGauge.GetValue(watcherEvent.Key),
        };

        cache[watcherEvent.Key] = watcherStateInfo;
    }

    private void ProcessRedEvent(WatcherEvent<TKubernetesObject> watcherEvent)
    {
        WatcherStateInfo watcherStateInfo = new()
        {
            Key = watcherEvent.Key,
            WatchedKubernetesObjectType = typeof(TKubernetesObject),
            LastException = watcherEvent.Exception,
            LastEventMoment = DateTime.UtcNow,
            Status = WatcherStateStatus.Red,
            EventsGauge = eventsGauge.GetValue(watcherEvent.Key),
        };

        cache[watcherEvent.Key] = watcherStateInfo;
    }

    private void ProcessFlushedEvent(WatcherEvent<TKubernetesObject> watcherEvent) =>
        cache.TryRemove(watcherEvent.Key, out _);
}
