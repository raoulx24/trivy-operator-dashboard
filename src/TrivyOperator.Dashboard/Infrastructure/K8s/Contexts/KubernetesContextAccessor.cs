using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Infrastructure.K8s.Contexts.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.K8s.Contexts;

public sealed class KubernetesContextAccessor : IKubernetesContextAccessor
{
    private static readonly AsyncLocal<ContextName?> Current = new();

    public bool TryGetCurrent(out ContextName context)
    {
        if (Current.Value is { } value)
        {
            context = value;
            return true;
        }

        context = new ContextName();
        return false;
    }

    public IDisposable Push(ContextName context)
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
