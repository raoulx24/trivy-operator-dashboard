namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ControlId
{
    public string Value { get; }

    public ControlId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Control id is required.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
