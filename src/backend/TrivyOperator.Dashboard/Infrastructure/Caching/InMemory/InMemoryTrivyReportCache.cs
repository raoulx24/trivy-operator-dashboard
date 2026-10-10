using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.CacheEntryBuilders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public abstract class InMemoryTrivyReportCacheInMemoryEntityCache<TResource, TKey>(
    IResourceDictionaryCache<TKey, CacheEntry<TResource, TKey>> cache,
    ICacheEntryBuilder<TResource, TKey> cacheEntryBuilder,
    IKubernetesContextResolver contextResolver,
    ILogger<InMemoryTrivyReportCacheInMemoryEntityCache<TResource, TKey>> logger) :
    InMemoryEntityCache<TResource, TKey>(cache, cacheEntryBuilder, contextResolver, logger)
    where TResource: class, ITrivyReport<TResource, TKey>
    where TKey : notnull
{
    public override Task Upsert(
        TResource resource,
        CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();

        _ = ContextResolver.TryGetCurrentContext(out ContextName contextName);

        ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>> innerCache =
            Cache.GetOrAdd(
                contextName,
                _ => new ConcurrentDictionary<TKey, CacheEntry<TResource, TKey>>());

        while (true)
        {
            ctx.ThrowIfCancellationRequested();

            // if the key does not exist, try to add
            // if add fails, it means that in the meant time it appeared, so we have to merge
            if (!innerCache.ContainsKey(resource.Id))
            {
                CacheEntry<TResource, TKey> incomingEntry =
                    CacheEntryBuilder.ToCacheEntry(resource);
                
                if (innerCache.TryAdd(resource.Id, incomingEntry))
                    return Task.CompletedTask;
            }

            // here we should have a key
            // if not, it means that it was deleted in the meantime, so we go back to TryAdd
            if (!innerCache.TryGetValue(resource.Id, out CacheEntry<TResource, TKey>? currentEntry))
                continue;

            TResource merged = currentEntry.Entry.MergeFrom(resource);

            CacheEntry<TResource, TKey> mergedEntry;
            
            if (resource.IsOtherNewer(currentEntry.Entry))
            {
                // the other is newer, so we ignore the details, just update the Entry
                mergedEntry = new()
                {
                    Entry = merged,
                    EncodedDetails = currentEntry.EncodedDetails,
                };
            }
            else
            {
                // the input one (resource) is newer, so we create a full-fledged CacheEntry
                mergedEntry = CacheEntryBuilder.ToCacheEntry(merged);
            }
            
            if (innerCache.TryUpdate(resource.Id, mergedEntry, currentEntry))
                return Task.CompletedTask;
            
            // if the above TryUpdate fails, it means that currentEntry changed, so we start over
        }
    }
}
