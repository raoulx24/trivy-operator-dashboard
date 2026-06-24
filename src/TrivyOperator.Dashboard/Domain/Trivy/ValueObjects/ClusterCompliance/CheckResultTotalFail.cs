namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct CheckResultTotalFail
{
    public long Value { get; }

    public CheckResultTotalFail(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public override string ToString() => Value.ToString();
}
