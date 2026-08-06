using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

public class DefaultKubernetesContextResolver(IKubernetesClientFactory kubernetesClientFactory)
    : IKubernetesContextResolver
{
    private readonly ContextName currentContext = kubernetesClientFactory.GetCurrentContext();

    public bool TryResolveCurrentContext(out ContextName context)
    {
        context = currentContext;

        return true;
    }
}
