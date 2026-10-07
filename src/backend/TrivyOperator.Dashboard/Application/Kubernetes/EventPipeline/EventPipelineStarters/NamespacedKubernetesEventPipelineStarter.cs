using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;

public class NamespacedKubernetesEventPipelineStarter<TResource, TKey>(
    IKubernetesEventDispatcher<TResource, TKey> kubernetesEventDispatcher,
    IKubernetesBackgroundQueue<TResource, TKey> queue,
    ILogger<NamespacedKubernetesEventPipelineStarter<TResource, TKey>> logger
) : KubernetesEventPipelineStarter<TResource, TKey>(kubernetesEventDispatcher, queue, logger)
    where TResource : class, IEntity<TKey>;
