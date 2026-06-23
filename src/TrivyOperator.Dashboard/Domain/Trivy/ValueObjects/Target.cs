namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;

public readonly record struct Target(string Value)
{
    public override string ToString() => Value;
}
