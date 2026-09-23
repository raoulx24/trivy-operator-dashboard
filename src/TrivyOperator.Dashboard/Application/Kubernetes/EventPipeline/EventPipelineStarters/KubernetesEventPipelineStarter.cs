using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;

public class KubernetesEventPipelineStarter<TResource, TKey>(
    IKubernetesEventDispatcher<TResource, TKey> kubernetesEventDispatcher,
    IEventPipelineBackgroundQueue<TResource, TKey> queue,
    ILogger<KubernetesEventPipelineStarter<TResource, TKey>> logger
) : IKubernetesEventPipelineStarter
    where TResource : class, IEntity<TKey>
{
    public virtual void StartPipeline(CancellationToken ctx = default)
    {
        logger.LogInformation("Starting Kubernetes Events Pipeline for {kubernetesObjectType}", nameof(TResource));
        
        queue.StartQueue();
        kubernetesEventDispatcher.StartEventsProcessing(ctx);   
    }
}
