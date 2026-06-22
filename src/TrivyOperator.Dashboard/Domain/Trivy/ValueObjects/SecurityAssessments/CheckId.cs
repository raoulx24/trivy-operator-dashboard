namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

public readonly record struct CheckId
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public CheckId(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public CheckId() : this(Sentinel) { }
    
    public override string ToString() => Value;
}
