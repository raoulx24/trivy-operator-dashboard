using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.CveSubjects;

internal static class CveSubjectsCompositionResolver
{
    // CVE subjects have no use of their own - they are collected only when a feature consumes them
    internal static CveSubjectsCompositionMode Resolve(IConfiguration configuration)
    {
        // NVD consumes them only when gated (not Nvd:AllowAnyCve)
        bool isNvdConsumer = configuration.LoadUseNvd() && !configuration.LoadNvdAllowAnyCve();

        if (!configuration.LoadUseCveEnrichment() && !isNvdConsumer)
        {
            return CveSubjectsCompositionMode.NoConsumers;
        }

        if (!configuration.LoadUseAnyVulnerabilityReport())
        {
            return CveSubjectsCompositionMode.NoVulnerabilityReports;
        }

        // the subjects come from the vulnerability definitions store, kept only for watched reports (default context)
        return configuration.LoadWatchVulnerabilityReports()
            ? CveSubjectsCompositionMode.Enabled
            : CveSubjectsCompositionMode.NoDefaultContext;
    }
}

internal enum CveSubjectsCompositionMode
{
    NoConsumers,
    NoVulnerabilityReports,
    NoDefaultContext,
    Enabled,
}
