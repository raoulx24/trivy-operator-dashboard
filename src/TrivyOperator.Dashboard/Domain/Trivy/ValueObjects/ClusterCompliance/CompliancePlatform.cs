namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct CompliancePlatform
{
    public string Value { get; }

    public CompliancePlatform(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Platform is required.");

        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}