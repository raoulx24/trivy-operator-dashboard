namespace TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

public readonly record struct ResourceLocation
{
    public ContextName ContextName { get; init; } = new();
    public NamespaceName NamespaceName { get; init; } = new();

    public ResourceLocation(ContextName contextName, NamespaceName namespaceName)
    {
        ContextName = contextName;
        NamespaceName = namespaceName;
    }

    public ResourceLocation()
    {
        ContextName = new ContextName();
        NamespaceName = new NamespaceName();
    }
    
    public override string ToString() => $"Ctx: {ContextName} - Ns: {NamespaceName}";
}
