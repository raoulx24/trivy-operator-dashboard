using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer.Abstractions;

public interface IResourceMaterializer<in TKubernetesObject, TResource, TKey>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>
    where TResource : class, IEntity<TKey>
{
    Task<TResource?> Materialize(TKubernetesObject? source, CancellationToken ctx = default);
}
