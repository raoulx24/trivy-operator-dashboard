using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.EventPipeline.EventPipelineStarters;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;

public class KubernetesEventPipelineStarter<TResource, TKey>(
    IKubernetesEventDispatcher<TResource, TKey> kubernetesEventDispatcher,
    IKubernetesBackgroundQueue<TResource, TKey> queue,
    ILogger<KubernetesEventPipelineStarter<TResource, TKey>> logger
) : EventPipelineStarter<KubernetesEvent<TResource, TKey>>(kubernetesEventDispatcher, queue, logger)
    where TResource : class, IEntity<TKey>;
