using StackExchange.Redis;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.Caching.Distributed;

public static class DistributedCacheKeyExtensions
{
    // VR History
    private static string GetKey(this Snapshot s)
        => s.Key.GetKey();

    // change DistributedCacheVulnerabilityReportsHistoryStore.ParseSnapshotKey if this two changes
    private static string GetKey(this SnapshotKey sk) =>
        $"{SnapshotKeyPrefix}:{{{sk.NamespaceName.Value}}}:{DistributedCacheDigestCodec.Encode(sk.Digest)}:{sk.CvesHash.Value}";
    
    private static string GetUnprocessedKey(this SnapshotKey sk) =>
        $"{UnprocessedSnapshotKeyPrefix}:{{{sk.NamespaceName.Value}}}:{DistributedCacheDigestCodec.Encode(sk.Digest)}:{sk.CvesHash.Value}";
    
    public const string SnapshotKeyPrefix = "vr";
    public const string UnprocessedSnapshotKeyPrefix = "vr-unprocessed";
        
    public static RedisKey ToRedisKey(this Snapshot s) => (RedisKey)s.GetKey();
    
    extension(SnapshotKey sk)
    {
        public RedisKey ToRedisKey() => (RedisKey)sk.GetKey();

        public RedisKey ToUnprocessedRedisKey() => (RedisKey)sk.GetUnprocessedKey();
    }
    
    public static string GetSnapshotKeyPattern(NamespaceName namespaceName, Digest digest) =>
        $"{DistributedCacheKeyExtensions.SnapshotKeyPrefix}:{{{namespaceName.Value}}}:{DistributedCacheDigestCodec.Encode(digest)}:*";
    
    public static string GetUnprocessedSnapshotKeyPattern(NamespaceName namespaceName) =>
        $"{DistributedCacheKeyExtensions.UnprocessedSnapshotKeyPrefix}:{{{namespaceName.Value}}}:*";
    
    public static string GetSnapshotIndexesKeyPattern(NamespaceName namespaceName) =>
        $"{DistributedCacheKeyExtensions.SnapshotKeyPrefix}:{{{namespaceName.Value}}}:*";
    public static string GetSnapshotIndexesKeyPattern(NamespaceName namespaceName, Digest digest) =>
        $"{DistributedCacheKeyExtensions.SnapshotKeyPrefix}:{{{namespaceName.Value}}}:{DistributedCacheDigestCodec.Encode(digest)}:*";
    
    
    // Namespace History
    public const string NamespacesKey = "vr:namespaces";
    
    public static RedisKey ToNamespacesRedisKey()
        => (RedisKey)NamespacesKey;
}

internal static class DistributedCacheDigestCodec
{
    public static string Encode(Digest digest)
        => digest.Value.Replace(':', '_');

    public static Digest Decode(string value)
        => new(value.Replace('_', ':'));
}