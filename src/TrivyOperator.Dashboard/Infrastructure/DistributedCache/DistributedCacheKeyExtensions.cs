using TrivyOperator.Dashboard.Domain.VulnerabilityReportsHistory;
using TrivyOperator.Dashboard.Domain.VulnerabilityReportsHistory.ValueObjects;


namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache;

public static class DistributedCacheKeyExtensions
{
    public static string GetKey(this SnapshotIndexEntry sie)
        => $"vr:{{{sie.Key.NamespaceName}}}:{sie.Key.Digest}";

    public static string GetKey(this Snapshot s)
        => $"vr:{{{s.Key.NamespaceName}}}:{s.Key.Digest}:{s.Key.CvesHash}";

    public static string GetKey(this SnapshotKey sk) =>
        $"vr:{{{sk.NamespaceName}}}:{sk.Digest}:{sk.CvesHash}";
}
