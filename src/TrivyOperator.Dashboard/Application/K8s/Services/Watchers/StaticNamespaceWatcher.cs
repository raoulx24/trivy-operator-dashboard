using k8s.Models;
using TrivyOperator.Dashboard.Application.K8s.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.K8s.Pipeline;
using TrivyOperator.Dashboard.Application.K8s.Services.Watchers.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Utils;

namespace TrivyOperator.Dashboard.Application.K8s.Services.Watchers;

public class StaticNamespaceWatcher(
    IKubernetesBackgroundQueue<V1Namespace> backgroundQueue,
    IClusterScopedResourceService<V1Namespace, V1NamespaceList> kubernetesNamespaceService
) : IClusterScopedWatcher<V1Namespace>
{
    public async Task Add(CancellationToken cancellationToken, string watcherKey = CacheUtils.DefaultCacheRefreshKey)
    {
        IList<V1Namespace> kubernetesNamespaces = await kubernetesNamespaceService.GetResources();
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
