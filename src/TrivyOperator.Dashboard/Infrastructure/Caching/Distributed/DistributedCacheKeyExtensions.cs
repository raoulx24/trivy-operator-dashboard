using StackExchange.Redis;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;

public static class DistributedCacheKeyExtensions
{
    // VR History
    private static string GetKey(this Snapshot s)
        => s.Key.GetKey();

    // change DistributedCacheVulnerabilityReportsHistoryStore.ParseSnapshotKey if this two changes
    private static string GetKey(this SnapshotKey sk) =>
        $"{SnapshotKeyPrefix}:{{{DistributedCacheDigestCodec.Encode(sk.Digest)}}}:{sk.CvesHash.Value}";
    
    public const string SnapshotKeyPrefix = "vr";
        
    public static RedisKey ToRedisKey(this Snapshot s) => (RedisKey)s.GetKey();
    
    extension(SnapshotKey sk)
    {
        public RedisKey ToRedisKey() => (RedisKey)sk.GetKey();
    }
    
    public static string GetSnapshotKeyPattern(Digest digest) =>
        $"{DistributedCacheKeyExtensions.SnapshotKeyPrefix}:{{{DistributedCacheDigestCodec.Encode(digest)}}}:*";
    
    // Digests History
    public const string DigestsKey = "vr:namespaces";
    
    public static RedisKey ToDigestsRedisKey()
        => (RedisKey)DigestsKey;
}

internal static class DistributedCacheDigestCodec
{
    public static string Encode(Digest digest)
        => digest.Value.Replace(':', '_');

    public static Digest Decode(string value)
        => new(value.Replace('_', ':'));
}