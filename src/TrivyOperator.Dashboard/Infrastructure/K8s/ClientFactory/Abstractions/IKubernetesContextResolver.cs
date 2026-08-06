using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.ClientFactory.Abstractions;

public interface IKubernetesContextResolver
{
    bool TryResolveCurrentContext(out ContextName context);
}
