using TrivyOperator.Dashboard.Application.Kubernetes.ClientFactory.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts;

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
