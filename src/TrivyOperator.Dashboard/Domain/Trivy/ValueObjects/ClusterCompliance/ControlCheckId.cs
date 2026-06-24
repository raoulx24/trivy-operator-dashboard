namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ControlCheckId
{
    public string Value { get; }

    public ControlCheckId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Check id is required.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
