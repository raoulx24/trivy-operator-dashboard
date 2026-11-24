using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;

public interface IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    Task<ResourcePage<TKubernetesObject>> GetInitialResources(
        WatcherScope scope,
        string? continueToken,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetWatchList(
        WatcherScope scope,
        string? resourceVersion,
        int timeoutSeconds,
        CancellationToken cancellationToken = default
    );
}
