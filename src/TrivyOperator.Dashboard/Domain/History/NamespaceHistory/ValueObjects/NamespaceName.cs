namespace TrivyOperator.Dashboard.Domain.History.NamespaceHistory.ValueObjects;

public readonly record struct NamespaceName
{
    private const string Sentinel = "__none__";
    public string Value { get; }

    public NamespaceName(string value)
    {
        Value = string.Intern(value.ToLowerInvariant());
    }
    
    public NamespaceName() : this(Sentinel) { } // for default()
    
    public bool IsClusterScoped => Value == Sentinel;

    public override string ToString() => Value;
}
