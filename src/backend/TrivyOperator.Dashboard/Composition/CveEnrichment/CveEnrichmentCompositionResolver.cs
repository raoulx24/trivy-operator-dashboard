using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.CveEnrichment;

internal static class CveEnrichmentCompositionResolver
{
    // without vulnerability reports there are no CVEs to enrich
    // the subjects are collected by CVE Subjects, enabled whenever this is
    internal static CveEnrichmentCompositionMode Resolve(IConfiguration configuration)
    {
        if (!configuration.LoadUseCveEnrichment())
        {
            return CveEnrichmentCompositionMode.Disabled;
        }

        return configuration.LoadUseAnyVulnerabilityReport()
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
