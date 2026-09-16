using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Models.WatcherEvents;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Application.KubernetesEventPipeline.Services.ResourceWatches.Abstractions;

public interface IKubernetesResourceWatch<TKubernetesObjectList, TKubernetesObject>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>, new()
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    Task<TKubernetesObjectList> GetInitialResources(
        WatcherKey key,
        string? continueToken,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetWatchList(
        WatcherKey key,
        string? resourceVersion,
        int timeoutSeconds,
        CancellationToken cancellationToken = default
    );
}
