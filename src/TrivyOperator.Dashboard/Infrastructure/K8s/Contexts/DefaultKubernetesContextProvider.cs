using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

public class DefaultKubernetesContextProvider(IKubernetesClientFactory kubernetesClientFactory)
    : IKubernetesContextProvider
{
    private readonly ContextName currentContext = kubernetesClientFactory.GetCurrentContext();

    public bool TryGetCurrentContext(out ContextName context)
    {
        context = currentContext;

        return true;
    }
}
