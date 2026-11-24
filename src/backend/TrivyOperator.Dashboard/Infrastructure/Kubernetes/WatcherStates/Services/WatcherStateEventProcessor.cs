using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Models;
using TrivyOperator.Dashboard.Application.Shared.Cache.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Internals;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Services;

public class WatcherStateEventProcessor<TResource, TKey>(
    ICache<WatcherId, WatcherStateInfo> cache,
    IWatchRetryPolicy retryPolicy,
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
                eventsGauge.OffsetValue(kubernetesEvent.Scope, 1);
                ProcessGreenEvent(kubernetesEvent);
                break;
            case PipelineEventType.Deleted:
                eventsGauge.OffsetValue(kubernetesEvent.Scope, -1);
                ProcessGreenEvent(kubernetesEvent);
                break;
            case PipelineEventType.Modified:
            case PipelineEventType.Bookmark:
            // the initial list ended; covers empty namespaces, which emit no InitialAdded to turn Yellow into Green
            case PipelineEventType.Initialized:
            case PipelineEventType.WatcherConnected:
            // a skipped (malformed) object is a data problem, not a watcher one; the watch goes on
            case PipelineEventType.Skipped:
                ProcessGreenEvent(kubernetesEvent);
                break;
            case PipelineEventType.Flushed:
                eventsGauge.RemoveKey(kubernetesEvent.Scope);
                ProcessFlushedEvent(kubernetesEvent);
                break;
            // the data is kept on errors (only Flushed drops it), so the gauge is kept too
            case PipelineEventType.Error when retryPolicy.IsRed(kubernetesEvent.Failure):
                ProcessRedEvent(kubernetesEvent);
                break;
            case PipelineEventType.Error:
                ProcessYellowEvent(kubernetesEvent);
                break;
            case PipelineEventType.Throttled:
                ProcessYellowEvent(kubernetesEvent);
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
        WatcherId id = new(typeof(TResource), kubernetesEvent.Scope); 
        WatcherStateInfo watcherStateInfo = new(
            Id: id,
            Status: WatcherStateStatus.Green,
            LastException: null,
            LastEventMoment: DateTime.UtcNow,
            EventsGauge: eventsGauge.GetValue(kubernetesEvent.Scope));

        cache[id] = watcherStateInfo;
    }

    private void ProcessYellowEvent(KubernetesEvent<TResource, TKey> kubernetesEvent)
    {
        WatcherId id = new(typeof(TResource), kubernetesEvent.Scope);
        WatcherStateInfo watcherStateInfo = new(
            Id: id,
            Status: WatcherStateStatus.Yellow,
            // set for a recovering watcher (Error not yet Red), null when throttled by our own gate
            LastException: kubernetesEvent.Failure?.Exception,
            LastEventMoment: DateTime.UtcNow,
            EventsGauge: eventsGauge.GetValue(kubernetesEvent.Scope));

        cache[id] = watcherStateInfo;
    }

    private void ProcessRedEvent(KubernetesEvent<TResource, TKey> kubernetesEvent)
    {
        WatcherId id = new(typeof(TResource), kubernetesEvent.Scope);
        WatcherStateInfo watcherStateInfo = new(
            Id: id,
            Status: WatcherStateStatus.Red,
            LastException: kubernetesEvent.Failure?.Exception,
            LastEventMoment: DateTime.UtcNow,
            EventsGauge: eventsGauge.GetValue(kubernetesEvent.Scope));

        cache[id] = watcherStateInfo;
    }

    private void ProcessFlushedEvent(KubernetesEvent<TResource, TKey> kubernetesEvent)
    {
        WatcherId id = new(typeof(TResource), kubernetesEvent.Scope);
        cache.TryRemove(id, out _);
    }
}
