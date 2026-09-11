using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Composition.History;

internal static class HistoryCompositionResolver
{
    internal static HistoryCompositionMode Resolve(
        IConfiguration configuration)
    {
        Dictionary<string, bool> trivyReports = configuration.LoadEnabledTrivyReports();
        bool isVulnerabilityReportEnabled = trivyReports.GetValueOrDefault(nameof(VulnerabilityReport));
        if (!isVulnerabilityReportEnabled 
            || !configuration.LoadUseHistory() 
            || configuration.LoadUseFileRepository() 
            || !configuration.LoadUseDefaultContext())
        {
            return HistoryCompositionMode.Disabled;
        }

        return HistoryCompositionMode.DistributedCache;
    }
}

internal enum HistoryCompositionMode
{
    Disabled,
    DistributedCache,
}
