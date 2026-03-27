using TrivyOperator.Dashboard.Domain.DistributedCache;
using TrivyOperator.Dashboard.Domain.VrHistory;

namespace TrivyOperator.Dashboard.Infrastructure.DistributedCache;

public static class DistributedCacheKeyExtensions
{
    public static string GetKey(this VrMetadata metadata)
        => $"vrmeta:{{{metadata.NamespaceName}}}:{metadata.Digest}";

    public static string GetKey(this VrSnapshot snapshot)
        => $"vr:{{{snapshot.NamespaceName}}}:{snapshot.Digest}:{snapshot.CvesHash}";

    public static string GetKey(this VrImageLineage lineage)
        => $"vrimage:{lineage.NamespaceName}";
}
