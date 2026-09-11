namespace TrivyOperator.Dashboard.Application.Queries.BackendSettings.Options;

public class EnabledTrivyReportsOptions
{
    public bool ClusterComplianceReport { get; init; } = true;
    public bool ClusterConfigAuditReport { get; init; } = true;
    public bool ClusterInfraAssessmentReport { get; init; } = true;
    public bool ClusterRbacAssessmentReport { get; init; } = true;
    public bool ClusterSbomReport { get; init; } = true;
    public bool ClusterVulnerabilityReport { get; init; } = true;

    public bool ConfigAuditReport { get; init; } = true;
    public bool ExposedSecretReport { get; init; } = true;
    public bool InfraAssessmentReport { get; init; } = true;
    public bool SbomReport { get; init; } = true;
    public bool RbacAssessmentReport { get; init; } = true;
    public bool VulnerabilityReport { get; init; } = true;}
