using TrivyOperator.Dashboard.Application.Kubernetes.Contexts.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts;

public class DefaultKubernetesContextResolver(IContextProvider kubernetesClientFactory)
    : IKubernetesContextResolver
{
    private readonly ContextName currentContext = kubernetesClientFactory.GetDefaultContext();

    public bool TryGetCurrentContext(out ContextName context)
    {
        context = currentContext;

        return true;
    }
}
