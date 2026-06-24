namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ControlCommandId
{
    public string Value { get; }

    public ControlCommandId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Command id is required.");

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
