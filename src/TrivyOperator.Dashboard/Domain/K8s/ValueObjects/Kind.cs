namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct Kind
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public Kind(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public Kind() : this(Sentinel) { }

    public override string ToString() => Value;
}
