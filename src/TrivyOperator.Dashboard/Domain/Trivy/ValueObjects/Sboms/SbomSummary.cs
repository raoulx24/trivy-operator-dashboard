namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public readonly record struct SbomSummary
{
    public int ComponentsCount { get; }
    public int DependenciesCount { get; }

    public SbomSummary(
        int componentsCount,
        int dependenciesCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(componentsCount);
        ArgumentOutOfRangeException.ThrowIfNegative(dependenciesCount);

        ComponentsCount = componentsCount;
        DependenciesCount = dependenciesCount;
    }
}