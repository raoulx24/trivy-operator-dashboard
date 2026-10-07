using TrivyOperator.Dashboard.Application.Alerts.Abstractions;
using TrivyOperator.Dashboard.Application.Alerts.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.WatcherStates.Refresher;

public class WatcherStateAlertRefresh<TResource, TKey>(
    IAlertPublisher alertPublisher,
    ILogger<WatcherStateAlertRefresh<TResource, TKey>> logger
) : IKubernetesEventProcessor<TResource, TKey>
    where TResource : class, IEntity<TKey>
{
    private const string AlertEmitter = "Watcher";
    private static readonly HashSet<ResourceLocation> ActiveAlerts = [];

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
            case PipelineEventType.Error:
                await AddAlert(kubernetesEvent, ctx);
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
        if (ActiveAlerts.Contains(kubernetesEvent.Key))
        {
            return;
        }

        ActiveAlerts.Add(kubernetesEvent.Key);

        await alertPublisher.AddAlert(
            AlertEmitter,
            new Alert
            {
                Key = GetCacheKey(kubernetesEvent),
                Message = $"Watcher for {typeof(TResource).Name}, context {kubernetesEvent.Key.ContextName} and {kubernetesEvent.Key.NamespaceName} failed.",
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
        if (ActiveAlerts.Contains(kubernetesEvent.Key))
        {
            ActiveAlerts.Remove(kubernetesEvent.Key);

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
        new([typeof(TResource).Name, kubernetesEvent.Key.ContextName.Value, kubernetesEvent.Key.NamespaceName.Value,]);
}
