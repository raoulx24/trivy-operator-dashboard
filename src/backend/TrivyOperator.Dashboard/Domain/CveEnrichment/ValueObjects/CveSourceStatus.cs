namespace TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

// outcome of a source for one CVE
// - Ok: the source has data for the CVE
// - NotListed: the source answered, but does not list the CVE (e.g. not in KEV) - this is not "not exploited"
// - Error: the last fetch failed; data from a previous successful fetch may still be present
public readonly record struct CveSourceStatus
{
    private const string Sentinel = "N/A";

    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ok",
        "NotListed",
        "Error",
    };

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public CveSourceStatus(string? value)
    {
        Value = value is not null && KnownValues.TryGetValue(value.Trim(), out string? known) ? known : Sentinel;
    }

    public CveSourceStatus() : this(Sentinel) { }

    public override string ToString() => Value;
}

public static class CveSourceStatuses
{
    public static readonly CveSourceStatus Ok = new("Ok");
    public static readonly CveSourceStatus NotListed = new("NotListed");
    public static readonly CveSourceStatus Error = new("Error");
}
