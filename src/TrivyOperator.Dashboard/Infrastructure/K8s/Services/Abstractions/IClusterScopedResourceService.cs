using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

public interface
    IClusterScopedResourceService<TKubernetesObject, TKubernetesObjectList>
    : IKubernetesResourceService<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    Task<TKubernetesObject> GetResource(string resourceName, CancellationToken cancellationToken = default);

    Task<TKubernetesObjectList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        Action<Exception>? onError = null,
        CancellationToken cancellationToken = default
    );
}
