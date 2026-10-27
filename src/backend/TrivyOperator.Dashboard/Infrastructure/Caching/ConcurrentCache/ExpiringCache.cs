using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Application.Metrics.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;

// a plain cache that clears itself when not accessed for InMemoryCache:ExpireInMinutes
// (no per entry expiration - see CacheExpiration)
public class ExpiringCache<TKey, TValue> : Cache<TKey, TValue>, IDisposable
    where TKey : notnull
{
    private readonly CacheExpiration expiration;

    public ExpiringCache(IMetricsClient metricsClient, IOptions<InMemoryCacheOptions> options)
        : base(metricsClient)
    {
        expiration = new CacheExpiration(TimeSpan.FromMinutes(options.Value.ExpireInMinutes), Clear);
    }

    protected override void OnAccess() => expiration.OnAccess();

    protected override void OnClear() => expiration.OnClear();

    public void Dispose() => expiration.Dispose();
}
