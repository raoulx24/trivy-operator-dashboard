namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct ContainerName
{
    private const string Sentinel = "n/a";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ContainerName(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim().ToLowerInvariant();
    }

    public ContainerName() : this(Sentinel) { }

    public override string ToString() => Value;
}
