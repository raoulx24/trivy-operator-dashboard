namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

public readonly record struct Title
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public Title(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public Title() : this(Sentinel) { }

    public override string ToString() => Value;
}
