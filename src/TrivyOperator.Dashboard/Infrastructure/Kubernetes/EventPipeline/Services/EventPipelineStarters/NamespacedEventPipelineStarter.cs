using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters;

public class NamespacedEventPipelineStarter<TResource, TKey>(
    IEventPipelineDispatcher<TResource, TKey> eventPipelineDispatcher,
    IEventPipelineBackgroundQueue<TResource, TKey> queue,
    ILogger<NamespacedEventPipelineStarter<TResource, TKey>> logger
) : KubernetesEventPipelineStarter<TResource, TKey>(eventPipelineDispatcher, queue, logger)
    where TResource : class, IEntity<TKey>;
