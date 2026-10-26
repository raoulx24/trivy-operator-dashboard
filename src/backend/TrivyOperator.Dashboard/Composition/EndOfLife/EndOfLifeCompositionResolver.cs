using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Composition.EndOfLife;

internal static class EndOfLifeCompositionResolver
{
    // the subjects (os families and versions) come only from vulnerability reports
    internal static EndOfLifeCompositionMode Resolve(IConfiguration configuration)
    {
        if (!configuration.LoadUseEndOfLife())
        {
            return EndOfLifeCompositionMode.Disabled;
        }

        Dictionary<string, bool> enabledReports = configuration.LoadEnabledTrivyReports();

        return enabledReports.GetValueOrDefault(nameof(VulnerabilityReport)) ||
               enabledReports.GetValueOrDefault(nameof(ClusterVulnerabilityReport))
            ? EndOfLifeCompositionMode.Enabled
            : EndOfLifeCompositionMode.NoVulnerabilityReports;
    }
}

internal enum EndOfLifeCompositionMode
{
    Disabled,
    NoVulnerabilityReports,
    Enabled,
}
