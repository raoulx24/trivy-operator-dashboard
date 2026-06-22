namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;

public sealed record Scanner
(
    ScannerName Name,
    ScannerVendor Vendor,
    ScannerVersion Version
);

    
public readonly record struct ScannerName
{
    public string Value { get; }

    public ScannerName(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));
        
        Value = string.Intern(value);
    }
    
    public override string ToString() => Value;
}

public readonly record struct ScannerVendor
{
    public string Value { get; }
    
    public ScannerVendor(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));
        
        Value = string.Intern(value);
    }

    public override string ToString() => Value;
}

public readonly record struct ScannerVersion
{
    public string Value { get; }
    
    public ScannerVersion(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));
        
        Value = string.Intern(value);
    }

    public override string ToString() => Value;
}
