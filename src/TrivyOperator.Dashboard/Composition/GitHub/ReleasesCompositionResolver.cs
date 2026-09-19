using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.GitHub;

internal static class ReleasesCompositionResolver
{
    internal static GitHubCompositionMode Resolve(IConfiguration configuration)
    {
        return configuration.LoadUseGithub()
            ? GitHubCompositionMode.Enabled
            : GitHubCompositionMode.Disabled;
    }
}

internal enum GitHubCompositionMode
{
    Disabled,
    Enabled,
}
