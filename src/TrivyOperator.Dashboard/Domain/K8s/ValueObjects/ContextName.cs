namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct ContextName
{
    private const string Sentinel = "";
    public string Value { get; }
    public bool IsUnset => string.IsNullOrEmpty(Value);

    public ContextName(string? value)
    {
        Value = string.Intern(value?.Trim() ?? Sentinel);
    }

    public ContextName() : this(Sentinel) { }

    public override string ToString() => Value;
}
