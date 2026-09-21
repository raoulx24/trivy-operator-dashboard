namespace TrivyOperator.Dashboard.Domain.Releases.ValueObjects;

public readonly record struct ReleaseVersion
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ReleaseVersion(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim();
    }

    public ReleaseVersion() : this(Sentinel) { }

    public override string ToString() => Value;
}
