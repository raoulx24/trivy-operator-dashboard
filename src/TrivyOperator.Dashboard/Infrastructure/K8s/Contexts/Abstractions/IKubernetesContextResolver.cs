using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

public interface IKubernetesContextResolver
{
    bool TryGetCurrentContext(out ContextName context);
}
