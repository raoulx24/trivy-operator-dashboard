namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceType
{
    private const string Sentinel = "n/a";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ComplianceType(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim().ToLowerInvariant();
    }

    public ComplianceType() : this(Sentinel) { }

    public override string ToString() => Value;
}
