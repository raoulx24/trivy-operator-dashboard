using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventProcessors.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.Models;
using TrivyOperator.Dashboard.Application.Shared.EventDispatchers;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers;

public class KubernetesEventDispatcher<TResource, TKey>(
    IEnumerable<IKubernetesEventProcessor<TResource, TKey>> services,
    IEventPipelineBackgroundQueue<TResource, TKey> backgroundQueue,
    ILogger<KubernetesEventDispatcher<TResource, TKey>> logger
) : EventDispatcher<TResource, KubernetesEvent<TResource, TKey>>(services, backgroundQueue, logger),
    IKubernetesEventDispatcher<TResource, TKey>
    where TResource : class, IEntity<TKey>;
