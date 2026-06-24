namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ControlName
{
    public string Value { get; }

    public ControlName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Name is required.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
