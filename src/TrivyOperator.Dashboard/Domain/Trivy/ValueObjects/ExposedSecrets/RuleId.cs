namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ExposedSecrets;

public readonly record struct RuleId
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public RuleId(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public RuleId() : this(Sentinel) { }
    
    public override string ToString() => Value;
}
