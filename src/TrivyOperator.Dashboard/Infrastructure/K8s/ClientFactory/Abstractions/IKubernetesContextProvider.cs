using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

public interface IKubernetesContextProvider
{
    bool TryGetCurrentContext(out ContextName context);
}
