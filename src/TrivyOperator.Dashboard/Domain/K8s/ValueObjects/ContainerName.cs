namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct ContainerName
{
    public string Value { get; }

    public ContainerName(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
