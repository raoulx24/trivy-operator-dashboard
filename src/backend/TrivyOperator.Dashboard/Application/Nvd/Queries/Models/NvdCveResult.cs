namespace TrivyOperator.Dashboard.Application.Nvd.Queries.Models;

// outcome of an NVD lookup; Cve is set only for Found (its status is Ok, or Error - e.g. rate limited)
public sealed record NvdCveResult(NvdLookupOutcome Outcome, NvdCveDto? Cve)
{
    public static NvdCveResult Found(NvdCveDto cve) => new(NvdLookupOutcomes.Found, cve);

    public static NvdCveResult Of(NvdLookupOutcome outcome) => new(outcome, null);
}

// - Found: NVD was asked (or answered from the cache); the dto says Ok or Error
// - InvalidId: not a CVE id (CVE-YYYY-NNNN...)
// - NotSubject: a CVE id, but not in the current vulnerability reports - NVD is not asked
// - NotInNvd: NVD has no record of it (not cached - a new CVE may show up in NVD later)
// - Disabled: NVD lookups are disabled (or there are no vulnerability reports)
public readonly record struct NvdLookupOutcome
{
    private const string Sentinel = "N/A";

    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "Found",
        "InvalidId",
        "NotSubject",
        "NotInNvd",
        "Disabled",
    };

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public NvdLookupOutcome(string? value)
    {
        Value = value is not null && KnownValues.TryGetValue(value.Trim(), out string? known) ? known : Sentinel;
    }

    public NvdLookupOutcome() : this(Sentinel) { }

    public override string ToString() => Value;
}

public static class NvdLookupOutcomes
{
    public static readonly NvdLookupOutcome Found = new("Found");
    public static readonly NvdLookupOutcome InvalidId = new("InvalidId");
    public static readonly NvdLookupOutcome NotSubject = new("NotSubject");
    public static readonly NvdLookupOutcome NotInNvd = new("NotInNvd");
    public static readonly NvdLookupOutcome Disabled = new("Disabled");
}
