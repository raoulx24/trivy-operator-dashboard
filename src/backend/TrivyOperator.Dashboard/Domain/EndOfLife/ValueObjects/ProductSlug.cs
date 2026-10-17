namespace TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

// endoflife.date product identifier (e.g. alpine-linux, rhel)
public readonly record struct ProductSlug
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ProductSlug(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim().ToLowerInvariant();
    }

    public ProductSlug() : this(Sentinel) { }

    public override string ToString() => Value;
}
