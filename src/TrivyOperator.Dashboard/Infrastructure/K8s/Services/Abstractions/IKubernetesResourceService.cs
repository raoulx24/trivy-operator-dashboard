using k8s;
using k8s.Models;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

public interface IKubernetesResourceService<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default);
}
