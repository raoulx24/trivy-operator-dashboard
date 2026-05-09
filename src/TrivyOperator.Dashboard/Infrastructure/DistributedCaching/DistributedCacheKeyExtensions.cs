using StackExchange.Redis;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCaching;

public static class DistributedCacheKeyExtensions
{
    // VR History
    private static string GetKey(this Snapshot s)
        => s.Key.GetKey();

    // change DistributedCacheVulnerabilityReportsHistoryStore.ParseSnapshotKey if this two changes
    private static string GetKey(this SnapshotKey sk) =>
        $"{SnapshotKeyPrefix}:{{{sk.NamespaceName.Value}}}:{sk.Digest.Value}:{sk.CvesHash.Value}";
    
    private static string GetUnprocessedKey(this SnapshotKey sk) =>
        $"{UnprocessedSnapshotKeyPrefix}:{{{sk.NamespaceName.Value}}}:{sk.Digest.Value}:{sk.CvesHash.Value}";
    
    public const string SnapshotKeyPrefix = "vr";
    public const string UnprocessedSnapshotKeyPrefix = "vr-unprocessed";
        
    public static RedisKey ToRedisKey(this Snapshot s) => (RedisKey)s.GetKey();
    
    extension(SnapshotKey sk)
    {
        public RedisKey ToRedisKey() => (RedisKey)sk.GetKey();

        public RedisKey ToUnprocessedRedisKey() => (RedisKey)sk.GetUnprocessedKey();
    }
    
    
    // Namespace History
    public const string NamespacesKey = "vr:namespaces";
    
    public static RedisKey ToNamespacesRedisKey()
        => (RedisKey)NamespacesKey;
}
