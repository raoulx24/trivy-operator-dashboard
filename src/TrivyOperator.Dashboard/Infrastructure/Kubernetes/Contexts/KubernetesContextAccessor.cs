using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Contexts;

public sealed class KubernetesContextAccessor : IKubernetesContextAccessor
{
    private static readonly AsyncLocal<ContextName?> Current = new();

    public bool TryGetCurrentContext(out ContextName context)
    {
        if (Current.Value is { } value)
        {
            context = value;
            return true;
        }

        context = new ContextName();
        return false;
    }

    public IDisposable PushContext(ContextName context)
    {
        ContextName? previous = Current.Value;
        Current.Value = context;

        return new ContextScope(previous);
    }

    private sealed class ContextScope(ContextName? previous) : IDisposable
    {
        public void Dispose()
        {
            Current.Value = previous;
        }
    }
}
