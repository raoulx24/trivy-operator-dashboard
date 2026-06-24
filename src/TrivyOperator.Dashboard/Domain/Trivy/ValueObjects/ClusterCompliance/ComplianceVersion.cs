namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ComplianceVersion
{
    public string Value { get; }

    public ComplianceVersion(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Version is required.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
