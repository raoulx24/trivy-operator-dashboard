namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

public readonly record struct Description
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public Description(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public Description() : this(Sentinel) { }

    public override string ToString() => Value;
}
