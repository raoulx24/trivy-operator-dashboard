namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Severity : IComparable<Severity>
{
    private static readonly Dictionary<string, int> SeverityRank = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CRITICAL"] = 0,
        ["HIGH"]     = 1,
        ["MEDIUM"]   = 2,
        ["LOW"]      = 3,
        ["UNKNOWN"]  = 4,
        ["NONE"]     = 5,
    };

    public string Value { get; }

    public Severity(string value)
    {
        string normalized = value.ToUpperInvariant();

        if (!SeverityRank.TryGetValue(normalized, out _))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid severity.");

        Value = string.Intern(normalized);
    }

    public int Rank => SeverityRank[Value];

    public int CompareTo(Severity other) => Rank.CompareTo(other.Rank);

    public static bool operator <(Severity left, Severity right) => left.Rank > right.Rank;
    public static bool operator >(Severity left, Severity right) => left.Rank < right.Rank;
    public static bool operator <=(Severity left, Severity right) => left.Rank >= right.Rank;
    public static bool operator >=(Severity left, Severity right) => left.Rank <= right.Rank;

    public override string ToString() => Value;
}

public static class Severities
{
    public static readonly Severity Critical = new("CRITICAL");
    public static readonly Severity High     = new("HIGH");
    public static readonly Severity Medium   = new("MEDIUM");
    public static readonly Severity Low      = new("LOW");
    public static readonly Severity Unknown  = new("UNKNOWN");
    public static readonly Severity None     = new("NONE");
}