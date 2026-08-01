using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries.Builders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class InMemoryImageReportCache<TResource>(
    IResourceConcurrentDictionaryCache<Digest, CacheEntry<TResource, Digest>> cache,
    ICacheEntryBuilder<TResource, Digest> cacheEntryBuilder,
    ILogger<InMemoryImageReportCache<TResource>> logger)
    : InMemoryEntityCache<TResource, Digest>(cache, cacheEntryBuilder, logger)
    where TResource: class, IImageReport<TResource>
{
    public override async Task ClearByNamespace(
        ContextName contextName,
        NamespaceName ns,
        CancellationToken ctx = default)
    {
        logger.LogDebug(
            "ClearByNamespace - {objectType} - {cacheKey} - {namespace}",
            typeof(TResource).Name,
            contextName,
            ns);

        ctx.ThrowIfCancellationRequested();

        if (!cache.TryGetValue(
                contextName,
                out ConcurrentDictionary<Digest, CacheEntry<TResource, Digest>>? innerCache))
        {
            return;
        }

        foreach (Digest key in innerCache.Keys)
        {
            ctx.ThrowIfCancellationRequested();

            await RemoveOccurrencesByNamespace(innerCache, key, ns, ctx);
        }
    }
    
    public override Task Delete(
        ContextName contextName,
        Digest key,
        NamespaceName namespaceName,
        CancellationToken ctx = default)
    {
        logger.LogDebug(
            "Delete - {objectType} - {cacheKey} - {namespace}",
            typeof(TResource).Name,
            key,
            namespaceName);

        ctx.ThrowIfCancellationRequested();

        if (!cache.TryGetValue(
                contextName,
                out ConcurrentDictionary<Digest, CacheEntry<TResource, Digest>>? innerCache))
        {
            return Task.CompletedTask;
        }

        return RemoveOccurrencesByNamespace(innerCache, key, namespaceName, ctx);
    }
    
    private Task RemoveOccurrencesByNamespace(
        ConcurrentDictionary<Digest, CacheEntry<TResource, Digest>> innerCache,
        Digest key,
        NamespaceName namespaceName,
        CancellationToken ctx)
    {
        while (true)
        {
            ctx.ThrowIfCancellationRequested();

            if (!innerCache.TryGetValue(key, out CacheEntry<TResource, Digest>? oldEntry))
            {
                return Task.CompletedTask;
            }

            IReadOnlyList<ReportImageOccurrence> remainingOccurrences =
            [
                .. oldEntry.Entry.Occurrences
                    .Where(o => o.Metadata.NamespaceName != namespaceName)
            ];

            // Nothing changed
            if (remainingOccurrences.Count == oldEntry.Entry.Occurrences.Count)
            {
                return Task.CompletedTask;
            }

            // Last occurrence removed => remove whole report
            if (remainingOccurrences.Count == 0)
            {
                if (innerCache.TryRemove(
                        new KeyValuePair<Digest, CacheEntry<TResource, Digest>>(key, oldEntry)))
                {
                    return Task.CompletedTask;
                }

                // concurrent update, retry
                continue;
            }

            CacheEntry<TResource, Digest> newEntry = new()
            {
                Entry = oldEntry.Entry.WithOccurrences(remainingOccurrences),
                EncodedDetails = oldEntry.EncodedDetails
            };

            if (innerCache.TryUpdate(key, newEntry, oldEntry))
            {
                return Task.CompletedTask;
            }

            // concurrent update, retry
        }
    }
}
