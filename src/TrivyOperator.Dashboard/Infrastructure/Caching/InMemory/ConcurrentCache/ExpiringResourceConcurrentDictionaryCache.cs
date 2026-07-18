using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Infrastructure.Clients.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.InMemory.ConcurrentCache;

public class ExpiringResourceConcurrentDictionaryCache<TKey, TValue> :
    ResourceConcurrentDictionaryCache<TKey, TValue>,
    IDisposable
    where TKey : notnull
{
    private readonly TimeSpan expireAfter;
    private readonly TimeSpan checkInterval;

    private readonly Lock expirationLock = new();

    private Timer? expirationTimer;
    private DateTimeOffset lastAccess = DateTimeOffset.UtcNow;

    public ExpiringResourceConcurrentDictionaryCache(
        IMetricsClient metricsClient,
        IOptions<InMemoryCacheOptions> options)
        : base(metricsClient)
    {
        expireAfter = TimeSpan.FromMinutes(options.Value.ExpireInMinutes);

        checkInterval = TimeSpan.FromTicks(expireAfter.Ticks / 10);
    }

    public bool IsStale()
    {
        lock (expirationLock)
        {
            if (DateTimeOffset.UtcNow - lastAccess >= expireAfter)
            {
                return true;
            }

            Touch();

            return false;
        }
    }

    protected override void OnAccess()
    {
        lock (expirationLock)
        {
            Touch();
        }
    }

    protected override void OnClear()
    {
        lock (expirationLock)
        {
            StopExpirationTimer();
        }
    }

    private void Touch()
    {
        lastAccess = DateTimeOffset.UtcNow;

        EnsureExpirationTimer();
    }

    private void EnsureExpirationTimer()
    {
        if (expirationTimer is not null)
        {
            return;
        }

        expirationTimer = new Timer(
            _ => CheckExpiration(),
            null,
            checkInterval,
            checkInterval);
    }

    private void CheckExpiration()
    {
        lock (expirationLock)
        {
            if (DateTimeOffset.UtcNow - lastAccess < expireAfter)
            {
                return;
            }

            Clear();
        }
    }

    private void StopExpirationTimer()
    {
        expirationTimer?.Dispose();
        expirationTimer = null;
    }

    public void Dispose()
    {
        lock (expirationLock)
        {
            StopExpirationTimer();
        }
    }
}