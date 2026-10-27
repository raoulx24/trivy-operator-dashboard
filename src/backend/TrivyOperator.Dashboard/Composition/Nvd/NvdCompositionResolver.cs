using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.Nvd;

internal static class NvdCompositionResolver
{
    // only CVE subjects can be looked up, and they come only from vulnerability reports
    // the subjects are collected by CVE Subjects, enabled whenever this is
    internal static NvdCompositionMode Resolve(IConfiguration configuration)
    {
        if (!configuration.LoadUseNvd())
        {
            return NvdCompositionMode.Disabled;
        }

        return configuration.LoadUseAnyVulnerabilityReport()
            ? NvdCompositionMode.Enabled
            : NvdCompositionMode.NoVulnerabilityReports;
    }
}

internal enum NvdCompositionMode
{
    Disabled,
    NoVulnerabilityReports,
    Enabled,
}
