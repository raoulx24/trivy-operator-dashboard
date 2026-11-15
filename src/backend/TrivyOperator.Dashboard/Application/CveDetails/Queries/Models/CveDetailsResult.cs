namespace TrivyOperator.Dashboard.Application.CveDetails.Queries.Models;

// outcome of a CVE details lookup; Cve is set only for Found (its status is Ok, or Error - e.g. rate limited)
public sealed record CveDetailsResult(CveDetailsLookupOutcome Outcome, CveDetailsDto? Cve)
{
    public static CveDetailsResult Found(CveDetailsDto cve) => new(CveDetailsLookupOutcomes.Found, cve);

    public static CveDetailsResult Of(CveDetailsLookupOutcome outcome) => new(outcome, null);
}

// - Found: the provider was asked (or answered from the cache); the dto says Ok or Error
// - InvalidId: not a CVE id (CVE-YYYY-NNNN...)
// - NotSubject: a CVE id, but not in the current vulnerability reports - the provider is not asked
// - NotListed: the provider has no (published) record of it (not cached - a new CVE may show up later)
// - Disabled: CVE details lookups are disabled (or there are no vulnerability reports)
public readonly record struct CveDetailsLookupOutcome
{
    private const string Sentinel = "N/A";

    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "Found",
        "InvalidId",
        "NotSubject",
        "NotListed",
        "Disabled",
    };

    public string Value { get => field ?? Sentinel; }
    public bool IsValid => Value != Sentinel;

    public CveDetailsLookupOutcome(string? value)
    {
        Value = value is not null && KnownValues.TryGetValue(value.Trim(), out string? known) ? known : Sentinel;
    }

    public CveDetailsLookupOutcome() : this(Sentinel) { }

    public override string ToString() => Value;
}

public static class CveDetailsLookupOutcomes
{
    public static readonly CveDetailsLookupOutcome Found = new("Found");
    public static readonly CveDetailsLookupOutcome InvalidId = new("InvalidId");
    public static readonly CveDetailsLookupOutcome NotSubject = new("NotSubject");
    public static readonly CveDetailsLookupOutcome NotListed = new("NotListed");
    public static readonly CveDetailsLookupOutcome Disabled = new("Disabled");
}
