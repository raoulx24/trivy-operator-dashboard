using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.Nvd;

internal static class NvdCompositionResolver
{
    // only the CVEs of the vulnerability reports can be looked up (unless Nvd:AllowAnyCve)
    // - watched reports (default context): the CVE subjects, collected by CVE Subjects
    // - reports read on demand (multi-context, file repository): the slim reports of the current context
    internal static NvdCompositionMode Resolve(IConfiguration configuration)
    {
        if (!configuration.LoadUseNvd())
        {
            return NvdCompositionMode.Disabled;
        }

        // not gated - needs no subjects, so no vulnerability reports either
        if (configuration.LoadNvdAllowAnyCve())
        {
            return NvdCompositionMode.AnyCve;
        }

        if (!configuration.LoadUseAnyVulnerabilityReport())
        {
            return NvdCompositionMode.NoVulnerabilityReports;
        }

        return configuration.LoadWatchVulnerabilityReports()
            ? NvdCompositionMode.Enabled
            : NvdCompositionMode.EnabledOnDemand;
    }
}

internal enum NvdCompositionMode
{
    Disabled,
    NoVulnerabilityReports,
    Enabled,
    EnabledOnDemand,
    AnyCve,
}
