namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceDescription
{
    public string Value { get; }

    public ComplianceDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Description is required.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
