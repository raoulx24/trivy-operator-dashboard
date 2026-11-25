using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.ScanOnDemand;

internal static class ScanOnDemandCompositionResolver
{
    // like history: only in the default context, never with the file repository
    internal static bool IsEnabled(IConfiguration configuration)
        => configuration.LoadUseScanOnDemand()
           && !configuration.LoadUseFileRepository()
           && configuration.LoadUseDefaultContext();
}
