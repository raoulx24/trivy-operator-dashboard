namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ControlName
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ControlName(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public ControlName() : this(Sentinel) { }

    public override string ToString() => Value;
}
