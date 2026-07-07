namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ResourceUrl
{
    public Uri? Value { get; }
    public string? InitialValue { get; }

    public ResourceUrl(string? value)
    {
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri);
        
        Value = uri;
        InitialValue = value;
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;
}
