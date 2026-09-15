namespace TrivyOperator.Dashboard.Composition.Common;

internal static class CompositionLogger
{
    public static ILogger? Logger { get; private set; }

    public static void Initialize(ILogger logger)
    {
        Logger = logger;
    }
}
