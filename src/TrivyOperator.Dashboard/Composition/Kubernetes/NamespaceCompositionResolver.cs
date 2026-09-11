using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.Kubernetes;

internal static class NamespaceCompositionResolver
{
    internal static NamespaceCompositionMode Resolve(IConfiguration configuration)
    {
        bool useFileRepository = configuration.LoadUseFileRepository();

        if (useFileRepository)
        {
            return NamespaceCompositionMode.Disabled;
        }

        bool useStaticNamespaceService = configuration.LoadUseStaticNamespaceService();

        bool useDefaultContext = configuration.LoadUseDefaultContext();

        if (useStaticNamespaceService)
        {
            return useDefaultContext
                ? NamespaceCompositionMode.StaticDefaultContext
                : NamespaceCompositionMode.StaticMultiContext;
        }

        return useDefaultContext
            ? NamespaceCompositionMode.DynamicDefaultContext
            : NamespaceCompositionMode.DynamicMultiContext;
    }
}

internal enum NamespaceCompositionMode
{
    Disabled,
    StaticDefaultContext,
    StaticMultiContext,
    DynamicDefaultContext,
    DynamicMultiContext,
}
