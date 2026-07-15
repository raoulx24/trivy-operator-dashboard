using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.ResourceStore.Abstractions;

public interface IResourceStore<TResource, in TKey>
{
    Task Upsert(NamespaceName namespaceName, TResource resource, CancellationToken? ctx = null);

    Task Delete(NamespaceName namespaceName, TKey key, CancellationToken? ctx = null);

    Task<TResource?> Get(NamespaceName namespaceName, TKey key, CancellationToken? ctx = null);

    Task ClearByNamespace(NamespaceName namespaceName, CancellationToken? ctx = null);
}
