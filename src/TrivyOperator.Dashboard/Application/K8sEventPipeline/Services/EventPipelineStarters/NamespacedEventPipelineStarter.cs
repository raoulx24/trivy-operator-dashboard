using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventPipelineStarters;

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
