namespace TrivyOperator.Dashboard.Application.CveEnrichment.Models;

// outcome of a provider call (one request or one paged download)
public readonly record struct CveProviderStatus
{
    private const string Sentinel = "N/A";

    private static readonly HashSet<string> KnownValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ok",
        "NotModified",
        "Error",
    };

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public CveProviderStatus(string? value)
    {
        Value = value is not null && KnownValues.TryGetValue(value.Trim(), out string? known) ? known : Sentinel;
    }

    public CveProviderStatus() : this(Sentinel) { }

    public override string ToString() => Value;
}

public static class CveProviderStatuses
{
    public static readonly CveProviderStatus Ok = new("Ok");
    public static readonly CveProviderStatus NotModified = new("NotModified");
    public static readonly CveProviderStatus Error = new("Error");
}
