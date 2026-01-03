namespace TrivyOperator.Dashboard.Application.Services.Options;

public record KubernetesOptions
{
    public string KubeConfigFileName { get; init; } = string.Empty;
    public bool UseDefaultContext { get; set; } = true;
    public string NamespaceList { get; init; } = string.Empty;
    public bool TrivyUseClusterRbacAssessmentReport { get; init; } = true;

    public bool TrivyUseClusterComplianceReport { get; init; } = true;
    public bool TrivyUseClusterSbomReport { get; init; } = true;
    public bool TrivyUseClusterVulnerabilityReport { get; init; } = true;

    public bool TrivyUseConfigAuditReport { get; init; } = true;
    public bool TrivyUseExposedSecretReport { get; init; } = true;
    public bool TrivyUseInfraAssessmentReport { get; init; } = true;
    public bool TrivyUseSbomReport { get; init; } = true;
    public bool TrivyUseRbacAssessmentReport { get; init; } = true;
    public bool TrivyUseVulnerabilityReport { get; init; } = true;
}
