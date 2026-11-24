using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.WatchSessions.Failures.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Refresher;

public class WatcherStateAlertRefresh<TResource, TKey>(
    IAlertPublisher alertPublisher,
    IWatchRetryPolicy retryPolicy,
    ILogger<WatcherStateAlertRefresh<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    private const string AlertEmitter = "Watcher";
    private static readonly HashSet<WatcherScope> ActiveAlerts = [];

    public async Task ProcessEvent(KubernetesEvent<TResource, TKey> kubernetesEvent, CancellationToken ctx = default)
    {
        switch (kubernetesEvent.PipelineEventType)
        {
            case PipelineEventType.InitialAdded:
            case PipelineEventType.Added:
            case PipelineEventType.Deleted:
            case PipelineEventType.Modified:
            case PipelineEventType.Bookmark:
            case PipelineEventType.WatcherConnected:
            case PipelineEventType.Flushed:
            case PipelineEventType.Initialized:
                await RemoveAlert(kubernetesEvent, ctx);
                break;
            // alert only when the watcher is Red; a recovering (Yellow) watcher keeps an existing alert
            case PipelineEventType.Error when retryPolicy.IsRed(kubernetesEvent.Failure):
                await AddAlert(kubernetesEvent, ctx);
                break;
            case PipelineEventType.Error:
            case PipelineEventType.Throttled:
            // a skipped (malformed) object is a data problem, not a watcher one
            case PipelineEventType.Skipped:
                // keep an existing alert until the watcher actually recovers
                break;
            case PipelineEventType.Unknown:
                logger.LogWarning(
                    "{watcherEventType} event type {eventType} for {kubernetesObjectType}.",
                    kubernetesEvent.PipelineEventType.ToString(),
                    kubernetesEvent.PipelineEventType,
                    typeof(TResource).Name
                );
                break;
        }
    }

    private async ValueTask AddAlert(KubernetesEvent<TResource, TKey> kubernetesEvent, CancellationToken ctx = default)
    {
        if (ActiveAlerts.Contains(kubernetesEvent.Scope))
        {
            return;
        }

        ActiveAlerts.Add(kubernetesEvent.Scope);

        await alertPublisher.AddAlert(
            AlertEmitter,
            new Alert
            {
                Key = GetCacheKey(kubernetesEvent),
                Message = $"Watcher for {typeof(TResource).Name}, context {kubernetesEvent.Scope.ContextName} and {kubernetesEvent.Scope.NamespaceName} failed ({kubernetesEvent.Failure?.Category}).",
                Severity = Severity.Error,
                Category = "Watcher Failed",
            },
            ctx
        );
    }

    private async ValueTask RemoveAlert(
        KubernetesEvent<TResource, TKey> kubernetesEvent,
        CancellationToken cancellationToken
    )
    {
        if (ActiveAlerts.Contains(kubernetesEvent.Scope))
        {
            ActiveAlerts.Remove(kubernetesEvent.Scope);

            await alertPublisher.RemoveAlert(
                AlertEmitter,
                new Alert
                {
                    Key = GetCacheKey(kubernetesEvent),
                },
                cancellationToken
            );
        }
    }
    
    // TODO: change this to IReadOnlyList<string> and create a IEqual for it
    private static EmitterKey GetCacheKey(KubernetesEvent<TResource, TKey> kubernetesEvent) =>
        new([typeof(TResource).Name, kubernetesEvent.Scope.ContextName.Value, kubernetesEvent.Scope.NamespaceName.Value,]);
}
