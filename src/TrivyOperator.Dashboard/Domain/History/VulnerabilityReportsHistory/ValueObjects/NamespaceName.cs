namespace TrivyOperator.Dashboard.Domain.History.VulnerabilityReportsHistory.ValueObjects;

public readonly record struct NamespaceName
{
    public string Value { get; }

    public NamespaceName(string value)
    {
        Value = value.ToLowerInvariant();
    }
}
