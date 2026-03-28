namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache.Client;

public record DistributedCacheRetryOptions
{
    public int MaxRetries { get; set; } = 5;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(50);
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromMilliseconds(1000);
}
