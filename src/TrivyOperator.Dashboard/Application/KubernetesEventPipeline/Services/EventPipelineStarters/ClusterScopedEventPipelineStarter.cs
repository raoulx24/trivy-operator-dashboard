using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.WatcherRegistries.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.EventPipelineStarters;

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

        WatcherKey watcherKey = new(contextName, new NamespaceName());
        base.StartPipeline(ctx);
        foreach (IClusterScopedWatcherRegistry clusterScopedWatcherRegistry in clusterScopedWatcherRegistries)
        {
            clusterScopedWatcherRegistry.StartWatcher(watcherKey, ctx);    
        }
    }
}
