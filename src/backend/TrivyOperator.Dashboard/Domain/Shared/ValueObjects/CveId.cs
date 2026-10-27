namespace TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

// a CVE identifier (CVE-YYYY-NNNN...), same shape as in the CISA KEV schema; anything else (GHSA, distro ids) is not valid
// not interned - the number of distinct CVEs is high and they are short lived outside the caches
public readonly record struct CveId
{
    private const string Sentinel = "N/A";
    private const string Prefix = "CVE-";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public CveId(string? value)
    {
        string? normalized = value?.Trim().ToUpperInvariant();

        Value = IsWellFormed(normalized) ? normalized! : Sentinel;
    }

    public CveId() : this(Sentinel) { }

    public override string ToString() => Value;

    // CVE- + 4 digits year + - + 4 to 19 digits sequence
    private static bool IsWellFormed(string? value)
    {
        if (value is null || value.Length < 13 || value.Length > 28 || !value.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        ReadOnlySpan<char> year = value.AsSpan(4, 4);
        ReadOnlySpan<char> sequence = value.AsSpan(9);

        return value[8] == '-' && IsDigits(year) && IsDigits(sequence);
    }

    private static bool IsDigits(ReadOnlySpan<char> span)
    {
        foreach (char c in span)
        {
            if (!char.IsAsciiDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}
