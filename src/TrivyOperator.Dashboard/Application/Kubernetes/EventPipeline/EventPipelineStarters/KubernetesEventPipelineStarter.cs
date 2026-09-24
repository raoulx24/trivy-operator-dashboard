using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.EventPipelineStarters;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;

public class KubernetesEventPipelineStarter<TResource, TKey>(
    IKubernetesEventDispatcher<TResource, TKey> kubernetesEventDispatcher,
    IKubernetesBackgroundQueue<TResource, TKey> queue,
    ILogger<KubernetesEventPipelineStarter<TResource, TKey>> logger
) : EventPipelineStarter<KubernetesEvent<TResource, TKey>>(queue)
    where TResource : class, IEntity<TKey>
{
    public override void StartPipeline(CancellationToken ctx = default)
    {
        logger.LogInformation("Starting Kubernetes Events Pipeline for {kubernetesObjectType}", nameof(TResource));
        
        base.StartPipeline(ctx);
        kubernetesEventDispatcher.StartEventsProcessing(ctx);   
    }
}
