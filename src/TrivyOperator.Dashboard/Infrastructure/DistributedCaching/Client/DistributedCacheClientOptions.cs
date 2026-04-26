namespace TrivyOperator.Dashboard.Infrastructure.DistributedCaching.Client;

public record DistributedCacheClientOptions
{
    public string ConnectionString { get; init; } = "localhost:6379,ssl=False,abortConnect=False";
}
