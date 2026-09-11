using TrivyOperator.Dashboard.Composition.Configuration;

namespace TrivyOperator.Dashboard.Composition.Kubernetes;

internal static class KubernetesCompositionResolver
{
    internal static KubernetesCompositionMode Resolve(IConfiguration configuration)
    {
        if (configuration.LoadUseFileRepository())
        {
            return KubernetesCompositionMode.Disabled;
        }

        return configuration.LoadUseDefaultContext()
            ? KubernetesCompositionMode.DefaultContext
            : KubernetesCompositionMode.MultiContext;
    }

}

internal enum KubernetesCompositionMode
{
    Disabled,
    DefaultContext,
    MultiContext,
}
