using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;

public class NamespacedEventPipelineStarter<TResource, TKey>(
    IEventPipelineDispatcher<TResource, TKey> eventPipelineDispatcher,
    IEventPipelineBackgroundQueue<TResource, TKey> queue,
    ILogger<NamespacedEventPipelineStarter<TResource, TKey>> logger
) : KubernetesEventPipelineStarter<TResource, TKey>(eventPipelineDispatcher, queue, logger)
    where TResource : class, IEntity<TKey>;
