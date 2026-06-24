namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceType
{
    public string Value { get; }

    public ComplianceType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Compliance type is required.");

        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}