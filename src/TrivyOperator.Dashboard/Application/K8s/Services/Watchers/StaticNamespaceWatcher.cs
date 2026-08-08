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
    public async Task Add(WatcherKey key, CancellationToken ctx = default)
    {
        IList<V1Namespace> kubernetesNamespaces = await kubernetesNamespaceService.GetResources(ctx);
        foreach (V1Namespace kubernetesNamespace in kubernetesNamespaces)
        {
            WatcherEvent<V1Namespace> watcherEvent = new()
            {
                Key = key,
                KubernetesObject = kubernetesNamespace,
                WatcherEventType = WatcherEventType.Added,
                IsStatic = true,
            };

            await backgroundQueue.QueueBackgroundWorkItemAsync(watcherEvent, ctx);
        }
    }

    public Task Recreate(WatcherKey key, CancellationToken ctx = default) => Task.CompletedTask;
    public Task Delete(WatcherKey key, CancellationToken ctx = default) => Task.CompletedTask;
}
