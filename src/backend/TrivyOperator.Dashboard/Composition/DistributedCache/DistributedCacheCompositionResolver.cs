using TrivyOperator.Dashboard.Composition.History;
using TrivyOperator.Dashboard.Composition.ScanOnDemand;

namespace TrivyOperator.Dashboard.Composition.DistributedCache;

internal static class DistributedCacheCompositionResolver
{
    // the features that persist in Redis/Valkey; any of them on means the distributed cache is required
    internal static bool IsRequired(IConfiguration configuration)
        => HistoryCompositionResolver.Resolve(configuration) == HistoryCompositionMode.DistributedCache
           || ScanOnDemandCompositionResolver.IsEnabled(configuration);
}
