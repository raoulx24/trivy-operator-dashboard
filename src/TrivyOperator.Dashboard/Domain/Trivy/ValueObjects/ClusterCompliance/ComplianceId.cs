namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceId
{
    public string Value { get; }

    public ComplianceId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Compliance id is required.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}