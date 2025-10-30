using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Services.KubernetesEventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.Services.WatcherEvents.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Services;

namespace TrivyOperator.Dashboard.Application.Services.WatcherStates;

public class WatcherState<TKubernetesObject>(
    IConcurrentCache<string, WatcherStateInfo> cache,
    ILogger<WatcherState<TKubernetesObject>> logger) 
    : IKubernetesEventProcessor<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>
{
    public Task ProcessKubernetesEvent(IWatcherEvent<TKubernetesObject> watcherEvent, CancellationToken cancellationToken)
    {
        if (watcherEvent.IsStatic)
        {
            return Task.CompletedTask;
        }
        switch (watcherEvent.WatcherEventType)
        {
            case WatcherEventType.InitialAdded:
            case WatcherEventType.Added:
                eventsGauge.OffsetValue(watcherEvent.WatcherKey, 1);
                ProcessGreenEvent(watcherEvent);
                break;
            case WatcherEventType.Deleted:
                eventsGauge.OffsetValue(watcherEvent.WatcherKey, -1);
                ProcessGreenEvent(watcherEvent);
                break;
            case WatcherEventType.Modified:
            case WatcherEventType.Bookmark:
            case WatcherEventType.WatcherConnected:
                ProcessGreenEvent(watcherEvent);
                break;
            case WatcherEventType.Flushed:
                eventsGauge.RemoveKey(watcherEvent.WatcherKey);
                ProcessFlushedEvent(watcherEvent);
                break;
            case WatcherEventType.Error:
                eventsGauge.RemoveKey(watcherEvent.WatcherKey);
                ProcessRedEvent(watcherEvent);
                break;
            case WatcherEventType.Initialized:
                break;
            case WatcherEventType.Unknown:
                logger.LogWarning("{watcherEventType} event type for {kubernetesObjectType}.",
                    watcherEvent.WatcherEventType.ToString(), typeof(TKubernetesObject).Name);
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }

    private void ProcessGreenEvent(IWatcherEvent<TKubernetesObject> watcherEvent)
    {
        WatcherStateInfo watcherStateInfo = new()
        {
            WatcherKey = watcherEvent.WatcherKey,
            WatchedKubernetesObjectType = typeof(TKubernetesObject),
            LastException = null,
            LastEventMoment = DateTime.UtcNow,
            Status = WatcherStateStatus.Green,
            EventsGauge = eventsGauge.GetValue(watcherEvent.WatcherKey),
        };
        
        cache[GetCacheKey(watcherEvent)] = watcherStateInfo;
    }

    private void ProcessRedEvent(IWatcherEvent<TKubernetesObject> watcherEvent)
    {
        WatcherStateInfo watcherStateInfo = new()
        {
            WatcherKey = watcherEvent.WatcherKey,
            WatchedKubernetesObjectType = typeof(TKubernetesObject),
            LastException = watcherEvent.Exception,
            LastEventMoment = DateTime.UtcNow,
            Status = WatcherStateStatus.Red,
            EventsGauge = eventsGauge.GetValue(watcherEvent.WatcherKey),
        };
        
        cache[GetCacheKey(watcherEvent)] = watcherStateInfo;
    }

    private void ProcessFlushedEvent(IWatcherEvent<TKubernetesObject> watcherEvent)
    {
        cache.TryRemove(GetCacheKey(watcherEvent), out _);
    }
    
    private static string GetCacheKey(IWatcherEvent<TKubernetesObject> watcherEvent) =>
        $"{typeof(TKubernetesObject).Name}|{watcherEvent.WatcherKey}";

    private readonly DictionaryCounter eventsGauge = new();
}
