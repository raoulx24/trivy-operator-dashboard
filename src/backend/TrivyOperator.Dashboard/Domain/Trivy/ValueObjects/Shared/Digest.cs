namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Digest
{
    private const string Sentinel = "n/a";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public Digest(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}
