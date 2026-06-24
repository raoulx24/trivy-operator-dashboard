namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

public sealed record ComplianceMetadata(
    ComplianceId Id,
    ComplianceTitle Title,
    ComplianceDescription Description,
    ComplianceType Type,
    CompliancePlatform CompliancePlatform,
    ComplianceVersion Version,
    IReadOnlyCollection<ResourceUrl> RelatedResources,
    IReadOnlyCollection<Control> Controls
);
