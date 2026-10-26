namespace TrivyOperator.Dashboard.Domain.CveEnrichment.ValueObjects;

// ENISA EU Vulnerability Database identifier (e.g. EUVD-2021-34768)
public readonly record struct EuvdId
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public EuvdId(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim().ToUpperInvariant();
    }

    public EuvdId() : this(Sentinel) { }

    public override string ToString() => Value;
}
