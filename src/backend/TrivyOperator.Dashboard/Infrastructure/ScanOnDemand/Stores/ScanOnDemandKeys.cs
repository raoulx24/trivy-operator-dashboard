using StackExchange.Redis;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Infrastructure.History.Stores;

namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Stores;

// sod = scan on demand
// the result keys of a digest share a hash tag ({digest}), so they can be written in one transaction on a cluster
internal static class ScanOnDemandKeys
{
    private const string Prefix = "sod";

    // string, the JSON scan record
    public static RedisKey Scan(ScanId id) => (RedisKey)$"{Prefix}:scan:{id.Value}";

    // set of scan ids; members whose record expired are removed lazily on read
    public static RedisKey Scans() => (RedisKey)$"{Prefix}:scans";

    // hash: the result of a digest
    public static RedisKey Result(Digest digest) => (RedisKey)$"{Prefix}:result:{{{DistributedCacheDigestCodec.Encode(digest)}}}";

    // set of the scan ids that point to the digest, for the delete
    public static RedisKey ResultScans(Digest digest) => (RedisKey)$"{Prefix}:result:{{{DistributedCacheDigestCodec.Encode(digest)}}}:scans";
}
