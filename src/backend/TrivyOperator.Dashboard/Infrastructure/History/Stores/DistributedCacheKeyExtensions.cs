using StackExchange.Redis;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.Entities;
using TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Infrastructure.History.Stores;

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
    
    public static string GetAllSnapshotsKeyPattern() =>
        $"{DistributedCacheKeyExtensions.SnapshotKeyPrefix}:{{*}}:*";

    // Digests History
    // same key as VulnerabilityReportsHistoryV2Migration.DigestsKey ("vr:namespaces" is the pre-2.0 namespace index)
    public const string DigestsKey = "vr:digests";
    
    public static RedisKey ToDigestsRedisKey()
        => (RedisKey)DigestsKey;

    // digest -> first seen moment ("O" format); entries live and die with the ones in vr:digests
    // same key as VulnerabilityReportsHistoryV2Migration.DigestsFirstSeenAtKey
    public const string DigestsFirstSeenAtKey = "vr:digests:firstSeenAt";

    public static RedisKey ToDigestsFirstSeenAtRedisKey()
        => (RedisKey)DigestsFirstSeenAtKey;
}

internal static class DistributedCacheDigestCodec
{
    public static string Encode(Digest digest)
        => digest.Value.Replace(':', '_');

    public static Digest Decode(string value)
        => new(value.Replace('_', ':'));
}
