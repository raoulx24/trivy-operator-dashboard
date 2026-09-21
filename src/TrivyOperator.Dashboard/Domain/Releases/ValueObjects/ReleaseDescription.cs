namespace TrivyOperator.Dashboard.Domain.Releases.ValueObjects;

public readonly record struct ReleaseDescription
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ReleaseDescription(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim();
    }

    public ReleaseDescription() : this(Sentinel) { }

    public override string ToString() => Value;
}
