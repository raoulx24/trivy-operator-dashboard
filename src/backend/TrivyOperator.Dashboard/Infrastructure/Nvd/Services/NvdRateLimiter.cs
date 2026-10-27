namespace TrivyOperator.Dashboard.Infrastructure.Nvd.Services;

// sliding window over the last requests, shared by all NVD calls (singleton)
// fail fast - no queueing; a refused caller gets how long until a slot frees up
public class NvdRateLimiter
{
    private readonly Lock syncLock = new();
    private readonly Queue<DateTimeOffset> requests = new();

    public bool TryAcquire(int permitLimit, TimeSpan window, out TimeSpan retryAfter)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        lock (syncLock)
        {
            while (requests.TryPeek(out DateTimeOffset oldest) && now - oldest >= window)
            {
                requests.Dequeue();
            }

            // the limit may go down (api key removed); the extra requests age out of the window
            if (requests.Count >= permitLimit)
            {
                retryAfter = requests.Peek() + window - now;

                return false;
            }

            requests.Enqueue(now);
            retryAfter = TimeSpan.Zero;

            return true;
        }
    }
}
