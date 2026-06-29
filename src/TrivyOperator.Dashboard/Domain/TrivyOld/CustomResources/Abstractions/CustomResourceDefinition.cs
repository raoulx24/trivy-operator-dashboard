namespace TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;

public abstract class CustomResourceDefinition
{
    public abstract string Version { get; }

    public abstract string Group { get; }

    public abstract string PluralName { get; }

    public abstract string Kind { get; }

    public abstract string? Namespace { get; init; }
}
