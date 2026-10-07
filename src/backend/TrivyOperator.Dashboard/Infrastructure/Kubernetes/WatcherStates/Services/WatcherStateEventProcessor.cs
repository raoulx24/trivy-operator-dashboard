using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Internals;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Services;

public class WatcherStateEventProcessor<TResource, TKey>(
    ICache<WatcherId, WatcherStateInfo> cache,
    ILogger<WatcherStateEventProcessor<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    private readonly WatcherEventsGauge eventsGauge = new();

    public Task ProcessEvent(KubernetesEvent<TResource, TKey> kubernetesEvent, CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();
        
        switch (kubernetesEvent.PipelineEventType)
        {
            case PipelineEventType.InitialAdded:
            case PipelineEventType.Added:
                eventsGauge.OffsetValue(kubernetesEvent.Key, 1);
                ProcessGreenEvent(kubernetesEvent);
                break;
            case PipelineEventType.Deleted:
                eventsGauge.OffsetValue(kubernetesEvent.Key, -1);
                ProcessGreenEvent(kubernetesEvent);
                break;
            case PipelineEventType.Modified:
            case PipelineEventType.Bookmark:
            case PipelineEventType.WatcherConnected:
                ProcessGreenEvent(kubernetesEvent);
                break;
            case PipelineEventType.Flushed:
                eventsGauge.RemoveKey(kubernetesEvent.Key);
                ProcessFlushedEvent(kubernetesEvent);
                break;
            case PipelineEventType.Error:
                eventsGauge.RemoveKey(kubernetesEvent.Key);
                ProcessRedEvent(kubernetesEvent);
                break;
            case PipelineEventType.Initialized:
                break;
            case PipelineEventType.Unknown:
                logger.LogWarning(
                    "{watcherEventType} event type for {kubernetesObjectType}.",
                    kubernetesEvent.PipelineEventType.ToString(),
                    typeof(TResource).Name
                );
                break;
        }

        return Task.CompletedTask;
    }

    private void ProcessGreenEvent(KubernetesEvent<TResource, TKey> kubernetesEvent)
    {
        WatcherId id = new(typeof(TResource), kubernetesEvent.Key); 
        WatcherStateInfo watcherStateInfo = new(
            Id: id,
            Status: WatcherStateStatus.Green,
            LastException: null,
            LastEventMoment: DateTime.UtcNow,
            EventsGauge: eventsGauge.GetValue(kubernetesEvent.Key));

        cache[id] = watcherStateInfo;
    }

    private void ProcessRedEvent(KubernetesEvent<TResource, TKey> kubernetesEvent)
    {
        WatcherId id = new(typeof(TResource), kubernetesEvent.Key);
        WatcherStateInfo watcherStateInfo = new(
            Id: id,
            Status: WatcherStateStatus.Red,
            LastException: kubernetesEvent.Exception,
            LastEventMoment: DateTime.UtcNow,
            EventsGauge: eventsGauge.GetValue(kubernetesEvent.Key));

        cache[id] = watcherStateInfo;
    }

    private void ProcessFlushedEvent(KubernetesEvent<TResource, TKey> kubernetesEvent)
    {
        WatcherId id = new(typeof(TResource), kubernetesEvent.Key);
        cache.TryRemove(id, out _);
    }
}
