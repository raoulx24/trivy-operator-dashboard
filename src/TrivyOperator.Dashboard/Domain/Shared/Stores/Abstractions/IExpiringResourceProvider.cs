namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

public interface IExpiringResourceProvider<TResource, TKey>
{
    Task<TResource?> GetResource(TKey key, CancellationToken ctx = default);
    Task<IReadOnlyList<TResource>> GetResources(CancellationToken ctx = default);
    Task<IReadOnlyList<TResource>> GetResources(IEnumerable<TKey> keys, CancellationToken ctx = default);
    Task<TResource?> GetResourceSummary(TKey key, CancellationToken ctx = default);
    Task<IReadOnlyList<TResource>> GetResourceSummaries(CancellationToken ctx = default);
    Task<IReadOnlyList<TKey>> GetResourceIds(CancellationToken ctx = default);

    Task Clear(CancellationToken ctx = default);
}
