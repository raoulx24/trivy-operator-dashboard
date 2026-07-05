namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct Uid
{
    public string Value { get; }

    public Uid(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value;
    }

    public override string ToString() => Value;
}
