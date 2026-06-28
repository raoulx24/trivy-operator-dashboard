namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record Dependency(
    ComponentId Source,
    ComponentId Target);
