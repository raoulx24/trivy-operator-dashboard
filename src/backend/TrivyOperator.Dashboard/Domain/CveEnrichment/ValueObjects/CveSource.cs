namespace TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

// the external source a piece of CVE enrichment comes from
// the values come from the known set, so they are already interned literals
public readonly record struct CveSource
{
    private const string Sentinel = "N/A";

    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "Kev",
        "Epss",
        "Euvd",
        "Circl",
    };

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public CveSource(string? value)
    {
        Value = value is not null && KnownValues.TryGetValue(value.Trim(), out string? known) ? known : Sentinel;
    }

    public CveSource() : this(Sentinel) { }

    public override string ToString() => Value;
}

public static class CveSources
{
    public static readonly CveSource Kev = new("Kev");
    public static readonly CveSource Epss = new("Epss");
    public static readonly CveSource Euvd = new("Euvd");
    // on demand only, not part of the background enrichment
    public static readonly CveSource Circl = new("Circl");
}
