namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct ContextName
{
    private const string Sentinel = "default";
    public string Value { get; }
    public bool IsDefault { get; }

    public ContextName(string? value)
    {
        Value = string.Intern(value?.Trim() ?? Sentinel);
        IsDefault = string.IsNullOrWhiteSpace(value);
    }

    public ContextName() : this(Sentinel) { }

    public override string ToString() => Value;
    
}
