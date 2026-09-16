using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPipelineStarters.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPipelineStarters;

public class KubernetesEventPipelineStarter<TKubernetesObject>(
    IKubernetesEventDispatcher<TKubernetesObject> kubernetesEventDispatcher,
    IKubernetesBackgroundQueue<TKubernetesObject> queue,
    ILogger<KubernetesEventPipelineStarter<TKubernetesObject>> logger
) : IKubernetesEventPipelineStarter
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public virtual void StartPipeline(CancellationToken ctx = default)
    {
        logger.LogInformation("Starting Kubernetes Events Pipeline for {kubernetesObjectType}", nameof(TKubernetesObject));
        
        queue.StartQueue();
        kubernetesEventDispatcher.StartEventsProcessing(ctx);   
    }
}
