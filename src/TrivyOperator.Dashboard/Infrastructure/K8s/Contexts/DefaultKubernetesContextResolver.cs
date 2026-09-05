using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

public class DefaultKubernetesContextResolver(IKubernetesClientFactory kubernetesClientFactory)
    : IKubernetesContextResolver
{
    private readonly ContextName currentContext = kubernetesClientFactory.GetDefaultContext();

    public bool TryGetCurrentContext(out ContextName context)
    {
        context = currentContext;

        return true;
    }
}
