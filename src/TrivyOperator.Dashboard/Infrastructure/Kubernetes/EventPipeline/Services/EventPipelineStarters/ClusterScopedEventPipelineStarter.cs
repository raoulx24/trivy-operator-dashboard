using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventDispatchers.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.EventPipelineStarters;

public class ClusterScopedEventPipelineStarter<TKubernetesObject>(
    IKubernetesEventDispatcher<TKubernetesObject> kubernetesEventDispatcher,
    IKubernetesBackgroundQueue<TKubernetesObject> queue,
    IKubernetesContextResolver contextResolver,
    IEnumerable<IClusterScopedWatcherRegistry> clusterScopedWatcherRegistries,
    ILogger<KubernetesEventPipelineStarter<TKubernetesObject>> logger
) : KubernetesEventPipelineStarter<TKubernetesObject>(kubernetesEventDispatcher, queue, logger)
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
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
