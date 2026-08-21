using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;

namespace TrivyOperator.Dashboard.Application.K8s.Services.EventPipelineStarters;

// TODO: replace TKubernetesEventDispatcher with IKubernetesEventDispatcher<TKubernetesObject>
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
