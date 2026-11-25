namespace TrivyOperator.Dashboard.Domain.ScanOnDemand.ValueObjects;

// identifies one scan request; generated on submit, used in Redis keys and in the scan pod name
public readonly record struct ScanId
{
    private const string Sentinel = "n/a";
    private const string Format = "N";

    public string Value { get => field ?? Sentinel; }
    public bool IsValid => Value != Sentinel;

    // anything that is not a guid in the "N" format (32 hex digits) becomes the sentinel, so a value coming
    // from the API can be used safely in keys and names
    public ScanId(string? value)
    {
        Value = Guid.TryParseExact(value?.Trim(), Format, out Guid guid) ? guid.ToString(Format) : Sentinel;
    }

    public ScanId() : this(Sentinel) { }

    public static ScanId New() => new(Guid.NewGuid().ToString(Format));

    public override string ToString() => Value;
}
