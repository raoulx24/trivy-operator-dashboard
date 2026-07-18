namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public readonly record struct SbomSummary
{
    public int ComponentsCount { get; }
    public int DependenciesCount { get; }

    public SbomSummary(
        int? componentsCount,
        int? dependenciesCount)
    {
        ComponentsCount = componentsCount is > 0 ? componentsCount.Value : 0;
        DependenciesCount = dependenciesCount is > 0 ? dependenciesCount.Value : 0;
    }
    
    public SbomSummary() : this(0,0) { }
}