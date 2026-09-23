using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;
using TrivyOperator.Dashboard.Application.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Internals;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Services;

public class WatcherStateEventProcessor<TResource, TKey>(
    ICache<ResourceLocation, WatcherStateInfo> cache,
    ILogger<WatcherStateEventProcessor<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    private readonly WatcherEventsGauge eventsGauge = new();

    public Task ProcessKubernetesEvent(WatcherEvent<TResource, TKey> watcherEvent, CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
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
                    typeof(TResource).Name
                );
                break;
        }

        return Task.CompletedTask;
    }

    private void ProcessGreenEvent(WatcherEvent<TResource, TKey> watcherEvent)
    {
        WatcherStateInfo watcherStateInfo = new()
        {
            Key = watcherEvent.Key,
            WatchedKubernetesObjectType = typeof(TResource),
            LastException = null,
            LastEventMoment = DateTime.UtcNow,
            Status = WatcherStateStatus.Green,
            EventsGauge = eventsGauge.GetValue(watcherEvent.Key),
        };

        cache[watcherEvent.Key] = watcherStateInfo;
    }

    private void ProcessRedEvent(WatcherEvent<TResource, TKey> watcherEvent)
    {
        WatcherStateInfo watcherStateInfo = new()
        {
            Key = watcherEvent.Key,
            WatchedKubernetesObjectType = typeof(TResource),
            LastException = watcherEvent.Exception,
            LastEventMoment = DateTime.UtcNow,
            Status = WatcherStateStatus.Red,
            EventsGauge = eventsGauge.GetValue(watcherEvent.Key),
        };

        cache[watcherEvent.Key] = watcherStateInfo;
    }

    private void ProcessFlushedEvent(WatcherEvent<TResource, TKey> watcherEvent) =>
        cache.TryRemove(watcherEvent.Key, out _);
}
