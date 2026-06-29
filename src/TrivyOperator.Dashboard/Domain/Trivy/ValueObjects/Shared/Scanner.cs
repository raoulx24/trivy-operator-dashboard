namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public sealed record Scanner
(
    ScannerName Name,
    ScannerVendor Vendor,
    ScannerVersion Version
);

public readonly record struct ScannerName
{
    private const string Sentinel = "N/A";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ScannerName(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public ScannerName() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct ScannerVendor
{
    private const string Sentinel = "N/A";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ScannerVendor(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public ScannerVendor() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct ScannerVersion
{
    private const string Sentinel = "N/A";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ScannerVersion(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : string.Intern(value.Trim());
    }

    public ScannerVersion() : this(Sentinel) { }

    public override string ToString() => Value;
}
