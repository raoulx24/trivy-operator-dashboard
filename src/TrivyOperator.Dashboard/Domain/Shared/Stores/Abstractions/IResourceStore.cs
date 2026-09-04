using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

public interface IResourceStore<TResource, in TKey>
{
    Task Upsert(TResource resource, CancellationToken ctx = default);

    Task Delete(TKey key, Uid uid, CancellationToken ctx = default);

    Task<TResource?> Get(TKey key, CancellationToken ctx = default);

    Task ClearByNamespace(NamespaceName namespaceName, CancellationToken ctx = default);
}
