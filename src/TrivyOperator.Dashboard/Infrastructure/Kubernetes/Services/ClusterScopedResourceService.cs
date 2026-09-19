using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services;

public abstract class ClusterScopedResourceService<TKubernetesObject, TKubernetesObjectList>(
    IKubernetesClientFactory kubernetesClientFactory,
    IKubernetesContextResolver contextResolver
) : KubernetesResourceService<TKubernetesObject>(kubernetesClientFactory, contextResolver),
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
        CancellationToken cancellationToken = default
    );
}
