namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceTitle
{
    public string Value { get; }

    public ComplianceTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Compliance Title is required.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
