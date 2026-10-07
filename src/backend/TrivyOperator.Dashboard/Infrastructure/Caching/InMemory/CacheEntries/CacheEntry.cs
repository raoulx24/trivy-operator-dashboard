using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;

public sealed class CacheEntry<TEntity, TId>
where TEntity : IEntity<TId>
{
    public required TEntity Entry { get; init; }
    public byte[] EncodedDetails { get; init; } = [];
}
