using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Services.Abstractions;

public interface IKubernetesResourceService<TKubernetesObject>
    where TKubernetesObject : IKubernetesObject<V1ObjectMeta>, IMetadata<V1ObjectMeta>
{
    Task<IList<TKubernetesObject>> GetResources(CancellationToken cancellationToken = default);

    ContextName GetCurrentContext();
}
