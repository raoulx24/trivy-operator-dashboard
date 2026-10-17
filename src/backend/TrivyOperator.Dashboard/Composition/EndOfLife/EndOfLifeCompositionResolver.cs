using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.EndOfLife;

internal static class EndOfLifeCompositionResolver
{
    internal static EndOfLifeCompositionMode Resolve(IConfiguration configuration)
    {
        return configuration.LoadUseEndOfLife()
            ? EndOfLifeCompositionMode.Enabled
            : EndOfLifeCompositionMode.Disabled;
    }
}

internal enum EndOfLifeCompositionMode
{
    Disabled,
    Enabled,
}
