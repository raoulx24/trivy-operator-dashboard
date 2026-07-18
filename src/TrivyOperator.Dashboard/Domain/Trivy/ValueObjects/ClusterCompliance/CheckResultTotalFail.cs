namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct CheckResultTotalFail
{
    public int Value { get; }

    public CheckResultTotalFail(int? value)
    {
        if (value is not { } r)
        {
            Value = 0;
            return;
        }

        Value = r > 0 ? r : 0;
    }
    
    public CheckResultTotalFail() : this(0) { }

    public override string ToString() => Value.ToString();
}
