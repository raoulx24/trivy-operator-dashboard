namespace TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;

// sliding expiration of a whole cache - cleared when not accessed for expireAfter (checked every expireAfter / 10)
// the timer runs only while the cache is in use: started on access, stopped on clear
// clear is the cache's Clear, which calls back OnClear - Lock is reentrant, so this is safe from the timer
internal sealed class CacheExpiration(TimeSpan expireAfter, Action clear) : IDisposable
{
    private readonly TimeSpan checkInterval = TimeSpan.FromTicks(expireAfter.Ticks / 10);

    private readonly Lock expirationLock = new();

    private Timer? expirationTimer;
    private DateTimeOffset lastAccess = DateTimeOffset.UtcNow;

    // TODO: add in Options the possibility to switch between sliding or absolute expiration

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

    public void OnAccess()
    {
        lock (expirationLock)
        {
            Touch();
        }
    }

    public void OnClear()
    {
        lock (expirationLock)
        {
            StopExpirationTimer();
        }
    }

    public void Dispose()
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

            clear();
        }
    }

    private void StopExpirationTimer()
    {
        expirationTimer?.Dispose();
        expirationTimer = null;
    }
}
