using k8s;
using k8s.Autorest;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.K8s.UpstreamAbstractions;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public abstract class ClusterScopedResourceDomainService<TKubernetesObject, TKubernetesObjectList>(
    IKubernetesClientFactory kubernetesClientFactory,
    IServiceScopeFactory scopeFactory
) : KubernetesResourceDomainService<TKubernetesObject>(kubernetesClientFactory, scopeFactory),
    IClusterScopedResourceWatchDomainService<TKubernetesObject, TKubernetesObjectList>
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

    public abstract Task<HttpOperationResponse<TKubernetesObjectList>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null
    );
}
