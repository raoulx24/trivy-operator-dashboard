using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.K8s.Abstractions;

public interface IResourceStore<TResource, in TKey>
{
    Task Upsert(ContextName contextName, TResource resource, CancellationToken ctx = default);

    Task Delete(ContextName contextName, TKey key, Uid uid, CancellationToken ctx = default);

    Task<TResource?> Get(ContextName contextName, TKey key, CancellationToken ctx = default);
    
    // Task<TResource?> GetLight(ContextName contextName, TKey key, CancellationToken ctx = default);

    Task ClearByNamespace(ContextName contextName, NamespaceName namespaceName, CancellationToken ctx = default);
}
