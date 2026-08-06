using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.CacheEntries;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Persistence.Trivy.Builders.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class InMemoryImageReportCache<TResource>(
    IResourceConcurrentDictionaryCache<Digest, CacheEntry<TResource, Digest>> cache,
    ICacheEntryBuilder<TResource, Digest> cacheEntryBuilder,
    IKubernetesContextResolver contextResolver,
    ILogger<InMemoryImageReportCache<TResource>> logger)
    : InMemoryEntityCache<TResource, Digest>(cache, cacheEntryBuilder, contextResolver, logger)
    where TResource: class, IImageReport<TResource>
{
    public override Task ClearByNamespace(NamespaceName ns, CancellationToken ctx = default)
    {
        _ = ContextResolver.TryResolveCurrentContext(out ContextName contextName);
        
        logger.LogDebug(
            "ClearByNamespace - {objectType} - {cacheKey} - {namespace}",
            typeof(TResource).Name,
            contextName,
            ns);

        ctx.ThrowIfCancellationRequested();

        if (!Cache.TryGetValue(
                contextName,
                out ConcurrentDictionary<Digest, CacheEntry<TResource, Digest>>? innerCache))
        {
            return Task.CompletedTask;
        }

        foreach (Digest key in innerCache.Keys)
        {
            ctx.ThrowIfCancellationRequested();
            
            RemoveOccurrences(
                innerCache,
                key,
                o => o.Metadata.NamespaceName == ns,
                ctx);
        }

        return Task.CompletedTask;
    }
    
    public override Task Delete(Digest key, Uid uid, CancellationToken ctx = default)
    {
        _ = ContextResolver.TryResolveCurrentContext(out ContextName contextName);
        
        logger.LogDebug(
            "Delete - {objectType} - {cacheKey} - {uid}",
            typeof(TResource).Name,
            key,
            uid);

        ctx.ThrowIfCancellationRequested();

        if (!Cache.TryGetValue(
                contextName,
                out ConcurrentDictionary<Digest, CacheEntry<TResource, Digest>>? innerCache))
        {
            return Task.CompletedTask;
        }

        RemoveOccurrences(
            innerCache,
            key,
            o => o.Metadata.Uid == uid,
            ctx);

        return Task.CompletedTask;
    }
    
    
    
    private static void RemoveOccurrences(
        ConcurrentDictionary<Digest, CacheEntry<TResource, Digest>> innerCache,
        Digest key,
        Func<ReportImageOccurrence, bool> shouldRemove,
        CancellationToken ctx)
    {
        while (true)
        {
            ctx.ThrowIfCancellationRequested();

            if (!innerCache.TryGetValue(key, out CacheEntry<TResource, Digest>? oldEntry))
            {
                return;
            }

            IReadOnlyList<ReportImageOccurrence> remainingOccurrences =
            [
                .. oldEntry.Entry.Occurrences.Where(o => !shouldRemove(o))
            ];

            // Nothing changed
            if (remainingOccurrences.Count == oldEntry.Entry.Occurrences.Count)
            {
                return;
            }

            // Last occurrence removed => remove whole report
            if (remainingOccurrences.Count == 0)
            {
                if (innerCache.TryRemove(
                        new KeyValuePair<Digest, CacheEntry<TResource, Digest>>(key, oldEntry)))
                {
                    return;
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
                return;
            }

            // concurrent update, retry
        }
    }
}
