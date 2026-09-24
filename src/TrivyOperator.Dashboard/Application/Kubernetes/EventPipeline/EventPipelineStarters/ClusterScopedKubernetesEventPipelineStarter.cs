using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Application.Kubernetes.EventPipeline.EventPipelineStarters;

public class ClusterScopedKubernetesEventPipelineStarter<TResource, TKey>(
    IKubernetesEventDispatcher<TResource, TKey> kubernetesEventDispatcher,
    IKubernetesBackgroundQueue<TResource, TKey> queue,
    IKubernetesContextResolver contextResolver,
    IEnumerable<IClusterScopedWatcherRegistry> clusterScopedWatcherRegistries,
    ILogger<KubernetesEventPipelineStarter<TResource, TKey>> logger
) : KubernetesEventPipelineStarter<TResource, TKey>(kubernetesEventDispatcher, queue, logger)
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
