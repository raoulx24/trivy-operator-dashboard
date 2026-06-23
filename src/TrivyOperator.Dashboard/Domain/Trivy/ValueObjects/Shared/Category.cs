namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Category
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public Category(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public Category() : this(Sentinel) { }

    public override string ToString() => Value;
}