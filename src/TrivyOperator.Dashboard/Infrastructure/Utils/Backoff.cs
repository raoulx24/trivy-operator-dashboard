namespace TrivyOperator.Dashboard.Infrastructure.Utils;

public static class Backoff
{
    public static TimeSpan DecorrelatedJitter(
        TimeSpan previousDelay,
        TimeSpan baseDelay,
        TimeSpan maxDelay)
    {
        double minMs = baseDelay.TotalMilliseconds;
        double maxMs = previousDelay.TotalMilliseconds * 3;

        double nextMs = Random.Shared.NextDouble() * (maxMs - minMs) + minMs;
        double capped = Math.Min(nextMs, maxDelay.TotalMilliseconds);

        return TimeSpan.FromMilliseconds(capped);
    }
}
