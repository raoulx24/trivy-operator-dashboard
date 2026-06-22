namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct ResourceName
{
    public string Value { get; }

    public ResourceName(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
