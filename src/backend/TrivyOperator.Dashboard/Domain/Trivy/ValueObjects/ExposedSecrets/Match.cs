namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;

public readonly record struct Match
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public Match(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public Match() : this(Sentinel) { }
    
    public override string ToString() => Value;
}
