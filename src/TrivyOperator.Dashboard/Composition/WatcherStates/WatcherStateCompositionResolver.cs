using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.WatcherStates;

internal static class WatcherStateCompositionResolver
{
    internal static WatcherStateCompositionMode Resolve(
        IConfiguration configuration)
    {
        return configuration.LoadUseDefaultContext()
            ? WatcherStateCompositionMode.Enabled
            : WatcherStateCompositionMode.Disabled;
    }
}

internal enum WatcherStateCompositionMode
{
    Disabled,
    Enabled,
}
