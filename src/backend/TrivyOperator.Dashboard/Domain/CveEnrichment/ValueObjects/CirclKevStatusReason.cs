namespace TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

// status_reason of an entry in CIRCL Vulnerability-Lookup's aggregated KEV catalogs
// unknown values (the API may add new ones) become the sentinel instead of failing
public readonly record struct CirclKevStatusReason
{
    private const string Sentinel = "N/A";

    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "confirmed",
        "suspected",
        "disputed",
        "historical",
        "unknown",
        "withdrawn",
    };

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public CirclKevStatusReason(string? value)
    {
        Value = value is not null && KnownValues.TryGetValue(value.Trim(), out string? known) ? known : Sentinel;
    }

    public CirclKevStatusReason() : this(Sentinel) { }

    public override string ToString() => Value;
}

public static class CirclKevStatusReasons
{
    public static readonly CirclKevStatusReason Confirmed = new("confirmed");
    public static readonly CirclKevStatusReason Suspected = new("suspected");
    public static readonly CirclKevStatusReason Disputed = new("disputed");
    public static readonly CirclKevStatusReason Historical = new("historical");
    public static readonly CirclKevStatusReason Unknown = new("unknown");
    public static readonly CirclKevStatusReason Withdrawn = new("withdrawn");
}
