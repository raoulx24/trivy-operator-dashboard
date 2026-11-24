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
            // the initial list is in progress: Yellow until Initialized, so a (re)created watcher is not
            // reported Green while it is still loading
            case PipelineEventType.InitialAdded:
                eventsGauge.OffsetValue(kubernetesEvent.Scope, 1);
                SetState(kubernetesEvent, WatcherStateStatus.Yellow, WatcherStateReason.Listing);
                break;
            case PipelineEventType.Added:
                eventsGauge.OffsetValue(kubernetesEvent.Scope, 1);
                SetState(kubernetesEvent, WatcherStateStatus.Green, WatcherStateReason.None);
                break;
            case PipelineEventType.Deleted:
                eventsGauge.OffsetValue(kubernetesEvent.Scope, -1);
                SetState(kubernetesEvent, WatcherStateStatus.Green, WatcherStateReason.None);
                break;
            case PipelineEventType.Modified:
            case PipelineEventType.Bookmark:
            // the initial list ended; also covers empty namespaces, which emit no InitialAdded
            case PipelineEventType.Initialized:
            case PipelineEventType.WatcherConnected:
                SetState(kubernetesEvent, WatcherStateStatus.Green, WatcherStateReason.None);
                break;
            // a skipped (malformed) object is a data problem, not a watcher one; the status stays as it is
            // (it must not turn a listing watcher Green)
            case PipelineEventType.Skipped:
                break;
            case PipelineEventType.Flushed:
                eventsGauge.RemoveKey(kubernetesEvent.Scope);
                ProcessFlushedEvent(kubernetesEvent);
                break;
            // the data is kept on errors (only Flushed drops it), so the gauge is kept too
            case PipelineEventType.Error when retryPolicy.IsRed(kubernetesEvent.Failure):
                SetState(kubernetesEvent, WatcherStateStatus.Red, WatcherStateReason.Failed);
                break;
            case PipelineEventType.Error:
                SetState(kubernetesEvent, WatcherStateStatus.Yellow, WatcherStateReason.Recovering);
                break;
            case PipelineEventType.Throttled:
                SetState(kubernetesEvent, WatcherStateStatus.Yellow, WatcherStateReason.Throttled);
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

    private void SetState(
        KubernetesEvent<TResource, TKey> kubernetesEvent,
        WatcherStateStatus status,
        WatcherStateReason reason)
    {
        WatcherId id = new(typeof(TResource), kubernetesEvent.Scope);
        WatcherStateInfo watcherStateInfo = new(
            Id: id,
            Status: status,
            // set on Error events only
            LastException: kubernetesEvent.Failure?.Exception,
            LastEventMoment: DateTime.UtcNow,
            EventsGauge: eventsGauge.GetValue(kubernetesEvent.Scope),
            Reason: reason);

        cache[id] = watcherStateInfo;
    }

    private void ProcessFlushedEvent(KubernetesEvent<TResource, TKey> kubernetesEvent)
    {
        WatcherId id = new(typeof(TResource), kubernetesEvent.Scope);
        cache.TryRemove(id, out _);
    }
}
