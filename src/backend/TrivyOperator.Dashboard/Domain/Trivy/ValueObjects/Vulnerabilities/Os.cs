namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Vulnerabilities;

public sealed record Os
(
    OsFamily Family,
    OsName Name,
    bool? IsEndOfLife
);

public readonly record struct OsFamily
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public OsFamily(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public OsFamily() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct OsName
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public OsName(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public OsName() : this(Sentinel) { }

    public override string ToString() => Value;
}
