using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

public interface IKubernetesContextResolver
{
    bool TryGetCurrentContext(out ContextName context);
}
