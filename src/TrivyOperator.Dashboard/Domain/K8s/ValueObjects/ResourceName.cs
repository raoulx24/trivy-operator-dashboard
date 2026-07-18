namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct ResourceName
{
    private const string Sentinel = "n/a";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ResourceName(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim().ToLowerInvariant();
    }

    public ResourceName() : this(Sentinel) { }

    public override string ToString() => Value;
}
