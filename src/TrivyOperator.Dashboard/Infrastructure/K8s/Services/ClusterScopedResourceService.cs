using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services;

public abstract class ClusterScopedResourceService<TKubernetesObject, TKubernetesObjectList>(
    IKubernetesClientFactory kubernetesClientFactory,
    IServiceScopeFactory scopeFactory
) : KubernetesResourceService<TKubernetesObject>(kubernetesClientFactory, scopeFactory),
    IClusterScopedResourceWatchService<TKubernetesObject, TKubernetesObjectList>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    public override async Task<IList<TKubernetesObject>> GetResources(CancellationToken? cancellationToken = null)
    {
        TKubernetesObjectList kubernetesObjectList = await GetResourceList(cancellationToken: cancellationToken);
        return kubernetesObjectList.Items;
    }

    public abstract Task<TKubernetesObjectList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null
    );

    public abstract Task<TKubernetesObject> GetResource(
        string resourceName,
        CancellationToken? cancellationToken = null
    );

    public abstract IAsyncEnumerable<WatchEvent<TKubernetesObject>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        Action<Exception>? onError = null,
        CancellationToken? cancellationToken = null
    );
}
