namespace TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;

public interface IResourceProvider<TResource, TKey>
{
    Task<TResource?> GetResource(TKey key, CancellationToken ctx = default);
    Task<IReadOnlyList<TResource>> GetResources(CancellationToken ctx = default);
    Task<IReadOnlyList<TResource>> GetResourceSummaries(CancellationToken ctx = default);
    Task<IReadOnlyList<TKey>> GetResourceIds(CancellationToken ctx = default);
}
