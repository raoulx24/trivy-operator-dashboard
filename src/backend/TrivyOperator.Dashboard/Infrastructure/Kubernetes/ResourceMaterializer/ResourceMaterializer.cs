using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer;

public class ResourceMaterializer<TKubernetesObject, TResource, TKey>(
    IResourceMapper<TKubernetesObject, TResource> mapper
): IResourceMaterializer<TKubernetesObject, TResource, TKey>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>
    where TResource : class, IEntity<TKey>
{
    public TResource? Materialize(TKubernetesObject? source)
    {
        return source is null ? null : mapper.MapToDomain(source, null);
    }
}
