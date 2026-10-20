namespace TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

public readonly record struct NamespaceName
{
    private const string Sentinel = "__none__";
    public string Value { get; }

    public NamespaceName(string? value)
    {
        // null, empty and whitespace all mean cluster scoped, so they must be equal to NamespaceName()
        Value = string.Intern(string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim().ToLowerInvariant());
    }
    
    public NamespaceName() : this(Sentinel) { }
    
    public bool IsClusterScoped => string.IsNullOrEmpty(Value) || Value == Sentinel;

    public override string ToString() => Value ?? Sentinel;
}
