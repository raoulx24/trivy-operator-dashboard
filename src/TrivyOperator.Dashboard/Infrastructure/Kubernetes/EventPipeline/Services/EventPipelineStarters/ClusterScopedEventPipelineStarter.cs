using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters;

public class ClusterScopedEventPipelineStarter<TResource, TKey>(
    IEventPipelineDispatcher<TResource, TKey> eventPipelineDispatcher,
    IEventPipelineBackgroundQueue<TResource, TKey> queue,
    IKubernetesContextResolver contextResolver,
    IEnumerable<IClusterScopedWatcherRegistry> clusterScopedWatcherRegistries,
    ILogger<KubernetesEventPipelineStarter<TResource, TKey>> logger
) : KubernetesEventPipelineStarter<TResource, TKey>(eventPipelineDispatcher, queue, logger)
    where TResource : class, IEntity<TKey>
{
    public override void StartPipeline(CancellationToken ctx = default)
    {
        if (!contextResolver.TryGetCurrentContext(out ContextName contextName))
        {
            contextName = new ContextName();
        }

        ResourceLocation resourceLocation = new(contextName, new NamespaceName());
        base.StartPipeline(ctx);
        foreach (IClusterScopedWatcherRegistry clusterScopedWatcherRegistry in clusterScopedWatcherRegistries)
        {
            clusterScopedWatcherRegistry.StartWatcher(resourceLocation, ctx);    
        }
    }
}
