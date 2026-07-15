using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

public interface IResourceStore<TResource, in TKey>
{
    Task UpsertResource(NamespaceName namespaceName, TResource resource);

    Task DeleteResource(NamespaceName namespaceName, TResource resource);

    Task<TResource?> GetResource(NamespaceName namespaceName, TKey key);
    
}
