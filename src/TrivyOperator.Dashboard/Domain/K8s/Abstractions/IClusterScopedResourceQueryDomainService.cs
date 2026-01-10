using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IClusterScopedResourceQueryDomainService<TKubernetesObject, TKubernetesObjectList>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    Task<TKubernetesObject> GetResource(string resourceName, CancellationToken? cancellationToken = null);

    Task<TKubernetesObjectList> GetResourceList(
        int? pageLimit = null,
        string? continueToken = null,
        CancellationToken? cancellationToken = null);

    Task<IList<TKubernetesObject>> GetResources(CancellationToken? cancellationToken = null);
}
