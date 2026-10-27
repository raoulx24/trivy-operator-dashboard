using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.CveSubjects;

internal static class CveSubjectsCompositionResolver
{
    // CVE subjects have no use of their own - they are collected only when a feature consumes them
    internal static CveSubjectsCompositionMode Resolve(IConfiguration configuration)
    {
        if (!configuration.LoadUseCveEnrichment() && !configuration.LoadUseNvd())
        {
            return CveSubjectsCompositionMode.NoConsumers;
        }

        return configuration.LoadUseAnyVulnerabilityReport()
            ? CveSubjectsCompositionMode.Enabled
            : CveSubjectsCompositionMode.NoVulnerabilityReports;
    }
}

internal enum CveSubjectsCompositionMode
{
    NoConsumers,
    NoVulnerabilityReports,
    Enabled,
}
