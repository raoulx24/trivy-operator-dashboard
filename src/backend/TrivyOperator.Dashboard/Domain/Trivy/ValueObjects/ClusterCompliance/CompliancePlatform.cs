namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct CompliancePlatform
{
    private const string Sentinel = "n/a";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public CompliancePlatform(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim().ToLowerInvariant();
    }

    public CompliancePlatform() : this(Sentinel) { }

    public override string ToString() => Value;
}
