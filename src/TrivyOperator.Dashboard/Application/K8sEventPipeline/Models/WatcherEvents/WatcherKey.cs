using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Application.K8sEventPipeline.Models.WatcherEvents;

public readonly record struct WatcherKey
{
    public ContextName ContextName { get; init; } = new();
    public NamespaceName NamespaceName { get; init; } = new();

    public WatcherKey(ContextName contextName, NamespaceName namespaceName)
    {
        ContextName = contextName;
        NamespaceName = namespaceName;
    }

    public WatcherKey()
    {
        ContextName = new ContextName();
        NamespaceName = new NamespaceName();
    }
    
    public override string ToString() => $"Ctx: {ContextName} - Ns: {NamespaceName}";
}
