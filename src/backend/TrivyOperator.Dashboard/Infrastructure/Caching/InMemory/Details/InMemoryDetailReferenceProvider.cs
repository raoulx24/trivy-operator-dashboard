using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Shared.Details.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details;

// the detail ids referenced by the slim reports of a cache, for the housekeeping of a details store
public abstract class InMemoryDetailReferenceProvider<TSlim, TKey, TDetailId>(
    IResourceDictionaryCache<TKey, CacheEntry<TSlim, TKey>> cache
) : IDetailReferenceProvider<TDetailId>
    where TSlim : class, IEntity<TKey>
    where TKey : notnull
{
    public Task<IReadOnlyDictionary<ContextName, IReadOnlySet<TDetailId>>> GetReferencedIds(
        CancellationToken ctx = default)
    {
        Dictionary<ContextName, IReadOnlySet<TDetailId>> result = [];

        foreach (KeyValuePair<ContextName, ConcurrentDictionary<TKey, CacheEntry<TSlim, TKey>>> context in cache.ToArray())
        {
            ctx.ThrowIfCancellationRequested();

            HashSet<TDetailId> ids = [];

            foreach (CacheEntry<TSlim, TKey> entry in context.Value.Values)
            {
                ids.UnionWith(GetIds(entry.Entry));
            }

            result[context.Key] = ids;
        }

        return Task.FromResult<IReadOnlyDictionary<ContextName, IReadOnlySet<TDetailId>>>(result);
    }

    protected abstract IEnumerable<TDetailId> GetIds(TSlim slim);
}
