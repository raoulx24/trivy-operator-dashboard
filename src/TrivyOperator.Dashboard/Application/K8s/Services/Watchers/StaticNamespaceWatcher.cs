using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Services.BackgroundQueues.Abstractions;
using TrivyOperator.Dashboard.Application.K8s.Services.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Domain.K8s.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class StaticNamespaceWatcher(
    IKubernetesBackgroundQueue<V1Namespace> backgroundQueue,
    IClusterScopedResourceQueryDomainService<V1Namespace, V1NamespaceList> kubernetesNamespaceDomainService
) : IClusterScopedWatcher<V1Namespace>
{
    public async Task Add(CancellationToken cancellationToken, string watcherKey = CacheUtils.DefaultCacheRefreshKey)
    {
        IList<V1Namespace> kubernetesNamespaces = await kubernetesNamespaceDomainService.GetResources();
        foreach (V1Namespace kubernetesNamespace in kubernetesNamespaces)
        {
            WatcherEvent<V1Namespace> watcherEvent = new()
            {
                WatcherKey = watcherKey,
                KubernetesObject = kubernetesNamespace,
                WatcherEventType = WatcherEventType.Added,
                IsStatic = true,
            };

            await backgroundQueue.QueueBackgroundWorkItemAsync(watcherEvent, cancellationToken);
        }
    }

    public Task Recreate(CancellationToken cancellationToken, string watcherKey = "generic.Key") => Task.CompletedTask;
}
