namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public readonly record struct ResourceUrl
{
    public Uri Value { get; }

    public ResourceUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
            throw new ArgumentException($"Invalid url: {value}");

        Value = uri;
    }

    public override string ToString() => Value.ToString();
}
