using TrivyOperator.Dashboard.Composition.Configuration;

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

        return configuration.LoadUseAnyVulnerabilityReport()
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
