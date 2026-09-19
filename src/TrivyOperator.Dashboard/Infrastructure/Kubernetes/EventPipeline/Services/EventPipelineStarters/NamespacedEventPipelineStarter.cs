using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters;

public class NamespacedEventPipelineStarter<TKubernetesObject>(
    IKubernetesEventDispatcher<TKubernetesObject> kubernetesEventDispatcher,
    IKubernetesBackgroundQueue<TKubernetesObject> queue,
    ILogger<NamespacedEventPipelineStarter<TKubernetesObject>>
        logger
) : KubernetesEventPipelineStarter<TKubernetesObject>(
    kubernetesEventDispatcher,
    queue,
    logger
)
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
{ }
