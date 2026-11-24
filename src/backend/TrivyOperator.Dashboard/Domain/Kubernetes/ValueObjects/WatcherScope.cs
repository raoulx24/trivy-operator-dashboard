namespace TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

// what a watcher covers: a context plus one namespace, or the whole cluster (namespace not set)
// it is the identity of a watcher session (registry key, watcher state, alerts); where a resource lives is a
// ResourceLocation, which can differ (a cluster wide watcher sees resources from many namespaces)
public readonly record struct WatcherScope
{
    public ContextName ContextName { get; init; } = new();
    public NamespaceName NamespaceName { get; init; } = new();

    public WatcherScope(ContextName contextName, NamespaceName namespaceName)
    {
        ContextName = contextName;
        NamespaceName = namespaceName;
    }

    public WatcherScope()
    {
        ContextName = new ContextName();
        NamespaceName = new NamespaceName();
    }

    public bool IsClusterWide => NamespaceName.IsClusterScoped;

    // the location of the scope itself; used for events that carry no resource
    public ResourceLocation ToResourceLocation() => new(ContextName, NamespaceName);

    public override string ToString() => $"Ctx: {ContextName} - Ns: {NamespaceName}";
}
