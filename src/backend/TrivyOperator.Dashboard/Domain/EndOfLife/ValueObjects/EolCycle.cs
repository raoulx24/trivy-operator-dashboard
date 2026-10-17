namespace TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

// endoflife.date release cycle name (e.g. 3.18, 12, 22.04)
public readonly record struct EolCycle
{
    private const string Sentinel = "N/A";

    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public EolCycle(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value)
            ? Sentinel
            : value.Trim();
    }

    public EolCycle() : this(Sentinel) { }

    public override string ToString() => Value;
}
