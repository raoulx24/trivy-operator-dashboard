using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

public interface IKubernetesContextAccessor
{
    bool TryGetCurrent(out ContextName context);
    IDisposable Push(ContextName context);
}
