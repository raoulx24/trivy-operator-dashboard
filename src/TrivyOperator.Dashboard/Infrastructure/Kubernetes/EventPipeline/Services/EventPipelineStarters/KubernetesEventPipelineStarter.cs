using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters;

public class KubernetesEventPipelineStarter<TResource, TKey>(
    IEventPipelineDispatcher<TResource, TKey> eventPipelineDispatcher,
    IEventPipelineBackgroundQueue<TResource, TKey> queue,
    ILogger<KubernetesEventPipelineStarter<TResource, TKey>> logger
) : IKubernetesEventPipelineStarter
    where TResource : class, IEntity<TKey>
{
    public virtual void StartPipeline(CancellationToken ctx = default)
    {
        logger.LogInformation("Starting Kubernetes Events Pipeline for {kubernetesObjectType}", nameof(TResource));
        
        queue.StartQueue();
        eventPipelineDispatcher.StartEventsProcessing(ctx);   
    }
}
