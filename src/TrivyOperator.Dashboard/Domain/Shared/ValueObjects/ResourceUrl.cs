namespace TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

public readonly record struct ResourceUrl
{
    private const string Sentinel = "n/a";
    public Uri? Value { get; }
    public string InitialValue { get; }
    public bool IsValid => Value is not null;

    public ResourceUrl(string? value)
    {
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri);
        
        Value = uri;
        InitialValue = string.IsNullOrWhiteSpace(value) ? Sentinel : value;
    }

    public override string ToString() => InitialValue?.ToString() ?? string.Empty;
}
