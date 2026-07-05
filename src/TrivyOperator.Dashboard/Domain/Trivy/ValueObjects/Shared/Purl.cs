namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Purl
{
    private const string Sentinel = "N/A";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public Purl(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public Purl() : this(Sentinel) { }

    public override string ToString() => Value;
}
