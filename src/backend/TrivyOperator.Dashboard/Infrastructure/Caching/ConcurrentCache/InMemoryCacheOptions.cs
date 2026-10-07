namespace TrivyOperator.Dashboard.Infrastructure.Caching.ConcurrentCache;

public class InMemoryCacheOptions
{
    public int ExpireInMinutes { get; set; } = 10;
}
