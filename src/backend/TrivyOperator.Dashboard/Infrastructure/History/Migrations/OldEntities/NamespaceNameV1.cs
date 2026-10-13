namespace TrivyOperator.Dashboard.Infrastructure.History.Migrations.OldEntities;

public readonly record struct NamespaceNameV1
{
    public string Value { get; }

    public NamespaceNameV1(string value)
    {
        Value = value.ToLowerInvariant();
    }
}
