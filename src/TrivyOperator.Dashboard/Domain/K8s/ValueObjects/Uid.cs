namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct Uid
{
    public string Value { get; }

    public Uid(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        Value = value.ToLowerInvariant();
    }

    public Uid(Guid value)
    {
        Value = value.ToString().ToLowerInvariant();
    }
    
    public static Uid CreateUid() => new(Guid.NewGuid());

    public override string ToString() => Value;
}
