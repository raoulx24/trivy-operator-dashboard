using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Metrics.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;

public class ExpiringResourceDictionaryCache<TKey, TValue> :
    ResourceDictionaryCache<TKey, TValue>,
    IDisposable,
    IExpiringResourceDictionaryCache<TKey, TValue>
    where TKey : notnull
{
    private readonly CacheExpiration expiration;

    public ExpiringResourceDictionaryCache(
        IMetricsClient metricsClient,
        IOptions<InMemoryCacheOptions> options)
        : base(metricsClient)
    {
        expiration = new CacheExpiration(TimeSpan.FromMinutes(options.Value.ExpireInMinutes), Clear);
    }

    public bool IsStale() => expiration.IsStale();

    public void ClearIfStale()
    {
        if (IsStale()) Clear();
    }

    protected override void OnAccess() => expiration.OnAccess();

    protected override void OnClear() => expiration.OnClear();

    public void Dispose() => expiration.Dispose();
}
