using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.ResourceWatches;

public sealed class NamespacedResourceWatch<TKubernetesObjectList, TKubernetesObject>(
    INamespacedResourceService<TKubernetesObject, TKubernetesObjectList> resourceService
) : IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public Task<TKubernetesObjectList> GetInitialResources(
        WatcherKey key,
        string? continueToken,
        int pageSize,
        CancellationToken cancellationToken = default
    ) =>
        resourceService.GetResourceList(
            key.NamespaceName.Value,
            pageSize,
            continueToken,
            cancellationToken
        );

    public IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetWatchList(
        WatcherKey key,
        string? resourceVersion,
        int timeoutSeconds,
        CancellationToken cancellationToken = default
    ) =>
        resourceService.GetResourceWatchList(
            key.NamespaceName.Value,
            resourceVersion,
            timeoutSeconds,
            cancellationToken
        );
}
