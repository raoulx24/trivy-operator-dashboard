namespace TrivyOperator.Dashboard.Domain.Releases.ValueObjects;

public readonly record struct ReleaseName
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ReleaseName(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim();
    }

    public ReleaseName() : this(Sentinel) { }

    public override string ToString() => Value;
}
