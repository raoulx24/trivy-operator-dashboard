namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public readonly record struct SbomSerialNumber(string Value)
{
    public override string ToString() => Value;
}
