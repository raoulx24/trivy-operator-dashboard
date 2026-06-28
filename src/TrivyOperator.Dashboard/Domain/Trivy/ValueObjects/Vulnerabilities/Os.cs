namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects;

public sealed record Os
(
    OsFamily Family,
    OsName Name,
    bool? Eosl
);

public readonly record struct OsFamily
{
    public string Value { get; }

    public OsFamily(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));
        
        Value = string.Intern(value);
    }
    
    public override string ToString() => Value;
}

public readonly record struct OsName
{
    public string Value { get; }
    
    public OsName(string value)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty", nameof(value));
        
        Value = string.Intern(value);
    }

    public override string ToString() => Value;
}
