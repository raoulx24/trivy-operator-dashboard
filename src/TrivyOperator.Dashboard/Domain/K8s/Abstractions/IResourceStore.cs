using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IResourceStore<TResource, in TKey>
{
    Task Upsert(NamespaceName namespaceName, TResource resource, CancellationToken ctx = default);

    Task Delete(NamespaceName namespaceName, TKey key, CancellationToken ctx = default);

    Task<TResource?> Get(NamespaceName namespaceName, TKey key, CancellationToken ctx = default);

    Task ClearByNamespace(NamespaceName namespaceName, CancellationToken ctx = default);
}
