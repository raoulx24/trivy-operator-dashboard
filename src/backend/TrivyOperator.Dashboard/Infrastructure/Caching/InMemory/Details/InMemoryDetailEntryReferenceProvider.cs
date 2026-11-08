using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Shared.Details.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Details;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.Details;

// the detail ids referenced by the details of another details store, for the housekeeping of a details store
// (the same as InMemoryDetailReferenceProvider, but the referencing side is a details store, not a report cache)
public abstract class InMemoryDetailEntryReferenceProvider<TReferencing, TKey, TDetailId>(
    IResourceDictionaryCache<TKey, DetailEntry<TReferencing>> cache
) : IDetailReferenceProvider<TDetailId>
    where TKey : notnull
{
    public Task<IReadOnlyDictionary<ContextName, IReadOnlySet<TDetailId>>> GetReferencedIds(
        CancellationToken ctx = default)
    {
        Dictionary<ContextName, IReadOnlySet<TDetailId>> result = [];

        foreach (KeyValuePair<ContextName, ConcurrentDictionary<TKey, DetailEntry<TReferencing>>> context in cache.ToArray())
        {
            ctx.ThrowIfCancellationRequested();

            HashSet<TDetailId> ids = [];

            foreach (DetailEntry<TReferencing> entry in context.Value.Values)
            {
                ids.UnionWith(GetIds(entry.Value));
            }

            result[context.Key] = ids;
        }

        return Task.FromResult<IReadOnlyDictionary<ContextName, IReadOnlySet<TDetailId>>>(result);
    }

    protected abstract IEnumerable<TDetailId> GetIds(TReferencing referencing);
}
