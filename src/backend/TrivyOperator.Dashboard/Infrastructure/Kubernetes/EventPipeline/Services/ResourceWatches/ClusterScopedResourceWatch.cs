using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches;

public sealed class ClusterScopedResourceWatch<TKubernetesObjectList, TKubernetesObject>(
    IClusterScopedResourceService<TKubernetesObject, TKubernetesObjectList> resourceService
) : IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public Task<TKubernetesObjectList> GetInitialResources(
        ResourceLocation key,
        string? continueToken,
        int pageSize,
        CancellationToken cancellationToken = default
    ) =>
        resourceService.GetResourceList(
            pageSize,
            continueToken,
            cancellationToken
        );

    public IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetWatchList(
        ResourceLocation key,
        string? resourceVersion,
        int timeoutSeconds,
        CancellationToken cancellationToken = default
    ) =>
        resourceService.GetResourceWatchList(
            resourceVersion,
            timeoutSeconds,
            cancellationToken
        );
}
