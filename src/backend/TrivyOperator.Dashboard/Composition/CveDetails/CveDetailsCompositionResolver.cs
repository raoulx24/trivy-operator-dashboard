using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.CveDetails;

internal static class CveDetailsCompositionResolver
{
    // only the CVEs of the vulnerability reports can be looked up (unless CveDetails:AllowAnyCve)
    // - watched reports (default context): the CVE subjects, collected by CVE Subjects
    // - reports read on demand (multi-context, file repository): the slim reports of the current context
    internal static CveDetailsCompositionMode Resolve(IConfiguration configuration)
    {
        if (!configuration.LoadUseCveDetails())
        {
            return CveDetailsCompositionMode.Disabled;
        }

        // not gated - needs no subjects, so no vulnerability reports either
        if (configuration.LoadCveDetailsAllowAnyCve())
        {
            return CveDetailsCompositionMode.AnyCve;
        }

        if (!configuration.LoadUseAnyVulnerabilityReport())
        {
            return CveDetailsCompositionMode.NoVulnerabilityReports;
        }

        return configuration.LoadWatchVulnerabilityReports()
            ? CveDetailsCompositionMode.Enabled
            : CveDetailsCompositionMode.EnabledOnDemand;
    }
}

internal enum CveDetailsCompositionMode
{
    Disabled,
    NoVulnerabilityReports,
    Enabled,
    EnabledOnDemand,
    AnyCve,
}
