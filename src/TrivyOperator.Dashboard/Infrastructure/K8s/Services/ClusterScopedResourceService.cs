using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

public abstract class ClusterScopedResourceService<TKubernetesObject, TKubernetesObjectList>(
    IKubernetesClientFactory kubernetesClientFactory,
    IKubernetesContextAccessor contextAccessor
) : KubernetesResourceService<TKubernetesObject>(kubernetesClientFactory, contextAccessor),
    IClusterScopedResourceService<TKubernetesObject, TKubernetesObjectList>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public override async Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default)
    {
        TKubernetesObjectList kubernetesObjectList = await GetResourceList(cancellationToken: cancellationToken);
        return kubernetesObjectList.Items;
    }

    public abstract Task<TKubernetesObjectList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken cancellationToken = default
    );

    public abstract Task<TKubernetesObject> GetResource(
        string resourceName,
        CancellationToken cancellationToken = default
    );

    public abstract IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        Action<Exception>? onError = null,
        CancellationToken cancellationToken = default
    );
}
