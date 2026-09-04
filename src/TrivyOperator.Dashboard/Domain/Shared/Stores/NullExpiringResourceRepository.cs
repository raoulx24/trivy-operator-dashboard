using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Shared.Stores;

public sealed class NullExpiringResourceRepository<TResource, TKey>
    : IExpiringResourceRepository<TResource, TKey>
    where TResource : class
{
    public Task Upsert(TResource resource, CancellationToken ctx = default)
        => Task.CompletedTask;

    public Task Delete(TKey key, Uid uid, CancellationToken ctx = default)
        => Task.CompletedTask;

    public Task<TResource?> Get(TKey key, CancellationToken ctx = default)
        => Task.FromResult<TResource?>(null);

    public Task ClearByNamespace(NamespaceName namespaceName, CancellationToken ctx = default)
        => Task.CompletedTask;

    public Task<TResource?> GetResource(TKey key, CancellationToken ctx = default)
        => Task.FromResult<TResource?>(null);

    public Task<IReadOnlyList<TResource>> GetResources(CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyList<TResource>>([]);

    public Task<IReadOnlyList<TResource>> GetResources(IEnumerable<TKey> keys, CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyList<TResource>>([]);

    public Task<TResource?> GetResourceSummary(TKey key, CancellationToken ctx = default)
        => Task.FromResult<TResource?>(null);

    public Task<IReadOnlyList<TResource>> GetResourceSummaries(CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyList<TResource>>([]);

    public Task<IReadOnlyList<TKey>> GetResourceIds(CancellationToken ctx = default)
        => Task.FromResult<IReadOnlyList<TKey>>([]);

    public Task Clear(CancellationToken ctx = default)
        => Task.CompletedTask;
}
