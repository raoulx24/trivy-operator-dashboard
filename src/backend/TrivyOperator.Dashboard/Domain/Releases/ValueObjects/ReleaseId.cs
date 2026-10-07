namespace TrivyOperator.Dashboard.Domain.Releases.ValueObjects;

public readonly record struct ReleaseId
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ReleaseId(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim();
    }

    public ReleaseId() : this(Sentinel) { }

    public override string ToString() => Value;
}
