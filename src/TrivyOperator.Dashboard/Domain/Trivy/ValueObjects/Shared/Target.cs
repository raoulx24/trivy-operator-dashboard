namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Target
{
    private const string Sentinel = "n/a";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public Target(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim().ToLowerInvariant();
    }

    public Target() : this(Sentinel) { }

    public override string ToString() => Value;
}
