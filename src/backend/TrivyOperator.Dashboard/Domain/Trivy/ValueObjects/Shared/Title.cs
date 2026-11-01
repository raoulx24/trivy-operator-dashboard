namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public readonly record struct Title
{
    private const string Sentinel = "N/A";

    public string Value { get => field ?? Sentinel; }
    public bool IsValid => Value != Sentinel;

    public Title(string? value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public Title() : this(Sentinel) { }

    public override string ToString() => Value;
}
