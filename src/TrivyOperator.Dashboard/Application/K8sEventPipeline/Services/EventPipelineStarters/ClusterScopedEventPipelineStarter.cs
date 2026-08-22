using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventDispatchers.Abstractions;
using TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Services.EventPipelineStarters;

public class ClusterScopedEventPipelineStarter<TKubernetesObject>(
    IKubernetesEventDispatcher<TKubernetesObject> kubernetesEventDispatcher,
    IKubernetesBackgroundQueue<TKubernetesObject> queue,
    IKubernetesContextAccessor contextAccessor,
    IEnumerable<IClusterScopedWatcher> clusterScopedWatchers,
    ILogger<KubernetesEventPipelineStarter<TKubernetesObject>> logger
) : KubernetesEventPipelineStarter<TKubernetesObject>(kubernetesEventDispatcher, queue, logger)
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public override void StartPipeline(CancellationToken ctx = default)
    {
        if (!contextAccessor.TryGetCurrent(out ContextName contextName))
        {
            contextName = new ContextName();
        }

        WatcherKey watcherKey = new(contextName, new NamespaceName());
        base.StartPipeline(ctx);
        foreach (IClusterScopedWatcher clusterScopedWatcher in clusterScopedWatchers)
        {
            clusterScopedWatcher.StartWatcher(watcherKey, ctx);    
        }
    }
}
