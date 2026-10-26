using TrivyOperator.Dashboard.Composition.Configuration;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Composition.CveEnrichment;

internal static class CveEnrichmentCompositionResolver
{
    // without vulnerability reports there are no CVEs to enrich
    internal static CveEnrichmentCompositionMode Resolve(IConfiguration configuration)
    {
        if (!configuration.LoadUseCveEnrichment())
        {
            return CveEnrichmentCompositionMode.Disabled;
        }

        Dictionary<string, bool> enabledReports = configuration.LoadEnabledTrivyReports();

        return enabledReports.GetValueOrDefault(nameof(VulnerabilityReport)) ||
               enabledReports.GetValueOrDefault(nameof(ClusterVulnerabilityReport))
            ? CveEnrichmentCompositionMode.Enabled
            : CveEnrichmentCompositionMode.NoVulnerabilityReports;
    }
}

internal enum CveEnrichmentCompositionMode
{
    Disabled,
    NoVulnerabilityReports,
    Enabled,
}
