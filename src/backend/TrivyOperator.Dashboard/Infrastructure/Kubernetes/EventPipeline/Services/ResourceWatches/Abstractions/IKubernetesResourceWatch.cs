using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.EventPipeline.Services.ResourceWatches.Abstractions;

public interface IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    Task<TKubernetesObjectList> GetInitialResources(
        ResourceLocation key,
        string? continueToken,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetWatchList(
        ResourceLocation key,
        string? resourceVersion,
        int timeoutSeconds,
        CancellationToken cancellationToken = default
    );
}
