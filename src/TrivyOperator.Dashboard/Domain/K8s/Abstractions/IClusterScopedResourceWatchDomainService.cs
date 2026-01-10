using k8s;
using k8s.Autorest;
using k8s.Models;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface
    IClusterScopedResourceWatchDomainService<TKubernetesObject, TKubernetesObjectList> :
    IClusterScopedResourceQueryDomainService<TKubernetesObject, TKubernetesObjectList>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
    where TKubernetesObjectList : IKubernetesObject<V1ListMeta>, IItems<TKubernetesObject>
{
    Task<HttpOperationResponse<TKubernetesObjectList>> GetResourceWatchList(
        string? lastResourceVersion = null,
        int? timeoutSeconds = null,
        CancellationToken? cancellationToken = null);
}
