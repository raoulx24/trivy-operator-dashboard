namespace TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Options;

public class FileRepositoryOptions
{
    public string BasePath { get; set; } = string.Empty;
    public string ClusterComplianceReportCrSubpath { get; set; } = string.Empty;
    public string ClusterInfraAssessmentReportCrSubpath { get; set; } = "cluster_infra_assessment_reports";
    public string ClusterRbacAssessmentReportCrSubpath { get; set; } = string.Empty;
    public string ClusterSbomReportCrSubpath { get; set; } = "cluster_sbom_reports";
    public string ClusterVulnerabilityReportCrSubpath { get; set; } = "cluster_vulnerability_reports";
    public string ConfigAuditReportCrSubpath { get; set; } = string.Empty;
    public string ExposedSecretReportCrSubpath { get; set; } = "secret_reports";
    public string InfraAssessmentReportCrSubpath { get; set; } = string.Empty;
    public string RbacAssessmentReportCrSubpath { get; set; } = string.Empty;
    public string SbomReportCrSubpath { get; set; } = "sbom_reports";
    public string VulnerabilityReportCrSubpath { get; set; } = "vulnerability_reports";
}
