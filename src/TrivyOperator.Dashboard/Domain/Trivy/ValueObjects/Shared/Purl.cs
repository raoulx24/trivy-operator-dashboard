namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Purl(string Value)
{
    public override string ToString() => Value;
}