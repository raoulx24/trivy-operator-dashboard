namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ControlId
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ControlId(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public ControlId() : this(Sentinel) { }

    public override string ToString() => Value;
}
