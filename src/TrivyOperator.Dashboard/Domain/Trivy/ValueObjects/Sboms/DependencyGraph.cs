namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record DependencyGraph(
    IReadOnlyList<Dependency> Dependencies);
