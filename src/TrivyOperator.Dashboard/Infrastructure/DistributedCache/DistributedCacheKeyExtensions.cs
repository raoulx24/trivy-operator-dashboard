using StackExchange.Redis;
using TrivyOperator.Dashboard.Domain.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.VulnerabilityReportsHistory.ValueObjects;


namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache;

public static class DistributedCacheKeyExtensions
{
    public static string GetKey(this SnapshotIndexEntry sie)
        => $"vr:{{{sie.Key.NamespaceName}}}:{sie.Key.Digest}";

    public static string GetKey(this Snapshot s)
        => s.Key.GetKey();

    // change RedisVulnerabilityReportsHistoryStore.ParseSnapshotKey if this one changes
    public static string GetKey(this SnapshotKey sk) =>
        $"vr:{{{sk.NamespaceName}}}:{sk.Digest}:{sk.CvesHash}";
    
    public static RedisKey ToRedisKey(this Snapshot s)
        => (RedisKey)s.GetKey();
    
    public static RedisKey ToRedisKey(this SnapshotKey sk)
        => (RedisKey)sk.GetKey();
}
