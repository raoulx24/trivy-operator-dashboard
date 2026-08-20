using System.Collections.Immutable;

namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Severity : IComparable<Severity>
{
    private static readonly Dictionary<string, int> SeverityToId = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CRITICAL"] = 0,
        ["HIGH"]     = 1,
        ["MEDIUM"]   = 2,
        ["LOW"]      = 3,
        ["UNKNOWN"]  = 4,
        ["NONE"]     = 5,
    };

    private static readonly Dictionary<int, string> RankToSeverity =
        SeverityToId.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public string Value { get; }

    public int Rank => SeverityToId[Value];

    public Severity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Value = Severities.None.Value;
            return;
        }
        
        string normalized = value.ToUpperInvariant();

        if (!SeverityToId.ContainsKey(normalized))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid severity.");

        Value = string.Intern(normalized);
    }

    public Severity(int? rank)
    {
        if (rank is not { } r)
        {
            Value = Severities.None.Value;
            return;
        }

        if (!RankToSeverity.TryGetValue(r, out string? value))
            throw new ArgumentOutOfRangeException(nameof(rank), rank, "Invalid severity rank.");

        Value = value;
    }

    public int CompareTo(Severity other) => Rank.CompareTo(other.Rank);

    public static bool operator <(Severity left, Severity right) => left.Rank > right.Rank;
    public static bool operator >(Severity left, Severity right) => left.Rank < right.Rank;
    public static bool operator <=(Severity left, Severity right) => left.Rank >= right.Rank;
    public static bool operator >=(Severity left, Severity right) => left.Rank <= right.Rank;

    public override string ToString() => Value;

    private static readonly Lazy<ImmutableArray<Severity>> RankedSeveritiesLazy = new(
        () => [.. RankToSeverity.OrderBy(kvp => kvp.Key).Select(kvp => new Severity(kvp.Value)),]
    );

    public static ImmutableArray<Severity> RankedSeverities => RankedSeveritiesLazy.Value;
}

public static class Severities
{
    public static readonly Severity Critical = new(0);
    public static readonly Severity High     = new(1);
    public static readonly Severity Medium   = new(2);
    public static readonly Severity Low      = new(3);
    public static readonly Severity Unknown  = new(4);
    public static readonly Severity None     = new(5);
}