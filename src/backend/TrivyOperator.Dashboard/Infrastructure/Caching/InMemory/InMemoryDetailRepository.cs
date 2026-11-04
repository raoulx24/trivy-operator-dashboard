using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Details.Options;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.Stores;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory;

public class InMemoryDetailRepository<TDetail, TId>(
    IResourceDictionaryCache<TId, DetailEntry<TDetail>> cache,
    IKubernetesContextResolver contextResolver,
    IOptionsMonitor<DetailsOptions> options,
    ILogger<InMemoryDetailRepository<TDetail, TId>> logger)
    : IDetailRepository<TDetail, TId>
    where TDetail : class, IDetail<TId>
    where TId : notnull
{
    public Task Upsert(TDetail detail, Timestamp lastSeenAt, CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();

        _ = contextResolver.TryGetCurrentContext(out ContextName contextName);

        ConcurrentDictionary<TId, DetailEntry<TDetail>> innerCache = cache.GetOrAdd(
            contextName,
            _ => new ConcurrentDictionary<TId, DetailEntry<TDetail>>());

        Timestamp now = Timestamp.Now();
        TimeSpan touchRefresh = TimeSpan.FromSeconds(Math.Max(1, options.CurrentValue.TouchRefreshInSeconds));

        while (true)
        {
            ctx.ThrowIfCancellationRequested();

            if (!innerCache.TryGetValue(detail.Id, out DetailEntry<TDetail>? current))
            {
                DetailEntry<TDetail> added = new()
                {
                    Value = detail,
                    LastSeenAt = lastSeenAt,
                    TouchedAt = now,
                };

                if (innerCache.TryAdd(detail.Id, added))
                {
                    logger.LogDebug("New {detailType} {detailId}", typeof(TDetail).Name, detail.Id);

                    return Task.CompletedTask;
                }

                // added meanwhile
                continue;
            }

            bool isNewer = lastSeenAt > current.LastSeenAt;
            bool isTouchStale = now.Value - current.TouchedAt.Value >= touchRefresh;

            // nothing to say: not newer and touched recently, so no write
            if (!isNewer && !isTouchStale)
            {
                return Task.CompletedTask;
            }

            // an equal content keeps the stored instance
            bool keepsValue = !isNewer || EqualityComparer<TDetail>.Default.Equals(current.Value, detail);

            DetailEntry<TDetail> next = new()
            {
                Value = keepsValue ? current.Value : detail,
                LastSeenAt = isNewer ? lastSeenAt : current.LastSeenAt,
                TouchedAt = now,
            };

            if (innerCache.TryUpdate(detail.Id, next, current))
            {
                return Task.CompletedTask;
            }

            // changed meanwhile
        }
    }

    public Task<bool> RemoveIf(
        ContextName contextName,
        TId id,
        DetailEntry<TDetail> expected,
        CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();

        if (!cache.TryGetValue(contextName, out ConcurrentDictionary<TId, DetailEntry<TDetail>>? innerCache))
        {
            return Task.FromResult(false);
        }

        // removes only when the stored entry is still the expected instance
        return Task.FromResult(
            innerCache.TryRemove(new KeyValuePair<TId, DetailEntry<TDetail>>(id, expected)));
    }

    public Task<IReadOnlyDictionary<TId, TDetail>> GetDetails(
        IEnumerable<TId> ids,
        CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();

        _ = contextResolver.TryGetCurrentContext(out ContextName contextName);

        if (contextName == default)
        {
            contextName = new ContextName();
        }

        Dictionary<TId, TDetail> result = [];

        if (!cache.TryGetValue(contextName, out ConcurrentDictionary<TId, DetailEntry<TDetail>>? innerCache))
        {
            return Task.FromResult<IReadOnlyDictionary<TId, TDetail>>(result);
        }

        foreach (TId id in ids)
        {
            if (innerCache.TryGetValue(id, out DetailEntry<TDetail>? entry))
            {
                result[id] = entry.Value;
            }
        }

        return Task.FromResult<IReadOnlyDictionary<TId, TDetail>>(result);
    }

    public Task<IReadOnlyDictionary<ContextName, IReadOnlyDictionary<TId, DetailEntry<TDetail>>>> GetEntries(
        CancellationToken ctx = default)
    {
        ctx.ThrowIfCancellationRequested();

        Dictionary<ContextName, IReadOnlyDictionary<TId, DetailEntry<TDetail>>> result = [];

        foreach (KeyValuePair<ContextName, ConcurrentDictionary<TId, DetailEntry<TDetail>>> context in cache.ToArray())
        {
            result[context.Key] = context.Value.ToDictionary(static x => x.Key, static x => x.Value);
        }

        return Task.FromResult<IReadOnlyDictionary<ContextName, IReadOnlyDictionary<TId, DetailEntry<TDetail>>>>(result);
    }
}
