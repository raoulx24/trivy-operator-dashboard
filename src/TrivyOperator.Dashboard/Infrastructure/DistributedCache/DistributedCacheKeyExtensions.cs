using TrivyOperator.Dashboard.Domain.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.VulnerabilityReportsHistory.ValueObjects;


namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache;

public static class DistributedCacheKeyExtensions
{
    public static string GetKey(this SnapshotIndexEntry metadata)
        => $"vr:{{{metadata.NamespaceName}}}:{metadata.Digest}";

    public static string GetKey(this Snapshot snapshot)
        => $"vr:{{{snapshot.NamespaceName}}}:{snapshot.Digest}:{snapshot.CvesHash}";

    public static string GetKey(this SnapshotKey snapshotKey) =>
        $"vr:{{{snapshotKey.NamespaceName}}}:{snapshotKey.Digest}:{snapshotKey.CvesHash}";
}
