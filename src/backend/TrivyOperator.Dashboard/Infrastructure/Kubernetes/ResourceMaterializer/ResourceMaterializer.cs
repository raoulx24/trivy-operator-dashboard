using k8s;
using k8s.Models;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.ResourceMaterializer;

public class ResourceMaterializer<TKubernetesObject, TResource, TKey>(
    IResourceProvider<TResource, TKey> resourceProvider,
    IResourceMapper<TKubernetesObject, TResource> mapper,
    IResourceKeyProvider<TKubernetesObject, TKey> keyProvider
): IResourceMaterializer<TKubernetesObject, TResource, TKey>
    where TKubernetesObject : class, IKubernetesObject<V1ObjectMeta>
    where TResource : class, IEntity<TKey>
{
    public async Task<TResource?> Materialize(TKubernetesObject? source, CancellationToken ctx = default)
    {
        if (source is null)
            return null;
        
        TKey domainKey = keyProvider.GetKey(source);

        TResource? existing = await resourceProvider.GetResource(domainKey, ctx);
        return mapper.MapToDomain(source, existing);
    }
}
