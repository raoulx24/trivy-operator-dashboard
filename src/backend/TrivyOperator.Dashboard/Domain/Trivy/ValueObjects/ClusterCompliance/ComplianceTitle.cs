namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceTitle
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ComplianceTitle(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public ComplianceTitle() : this(Sentinel) { }

    public override string ToString() => Value;
}
