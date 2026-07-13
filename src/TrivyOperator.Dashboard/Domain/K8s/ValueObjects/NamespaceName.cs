namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct NamespaceName
{
    private const string Sentinel = "__none__";
    public string Value { get; }

    public NamespaceName(string? value)
    {
        Value = string.Intern(value?.ToLowerInvariant() ?? Sentinel);
    }
    
    public NamespaceName() : this(Sentinel) { }
    
    public bool IsClusterScoped => Value == Sentinel;

    public override string ToString() => Value ?? Sentinel;
}
