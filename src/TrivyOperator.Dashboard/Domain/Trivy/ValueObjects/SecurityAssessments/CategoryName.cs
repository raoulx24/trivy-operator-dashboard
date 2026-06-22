namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

public readonly record struct CategoryName
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public CategoryName(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public CategoryName() : this(Sentinel) { }

    public override string ToString() => Value;
}