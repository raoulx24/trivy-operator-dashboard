using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

public class DefaultKubernetesContextProvider(IKubernetesClientFactory kubernetesClientFactory)
    : IKubernetesContextProvider
{
    private readonly string currentContext = kubernetesClientFactory.GetCurrentContext();

    public bool TryGetCurrentContext(out string? context)
    {
        context = currentContext;

        return true;
    }
}
