using TrivyOperator.Dashboard.Domain.Trivy.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;

namespace TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository;

public class FolderNameFactory : IFolderNameFactory
{
    public string Get<TKubernetesObject>()
        => typeof(TKubernetesObject) switch
        {
            //{ } t when t == typeof(ClusterComplianceReportCr) => "",
            { } t when t == typeof(ClusterInfraAssessmentReportCr) => "cluster_infra_assessment_reports",
            //{ } t when t == typeof(ClusterRbacAssessmentReportCr) => "",
            { } t when t == typeof(ClusterSbomReportCr) => "cluster_sbom_reports",
            { } t when t == typeof(ClusterVulnerabilityReportCr) => "cluster_vulnerability_reports",
            //{ } t when t == typeof(ConfigAuditReportCr) => "",
            { } t when t == typeof(ExposedSecretReportCr) => "secret_reports",
            //{ } t when t == typeof(InfraAssessmentReportCr) => "",
            //{ } t when t == typeof(RbacAssessmentReportCr) => "",
            { } t when t == typeof(SbomReportCr) => "sbom_reports",
            { } t when t == typeof(VulnerabilityReportCr) => "vulnerability_reports",
            _ => throw new InvalidOperationException($"Unsupported Kubernetes object type - {typeof(TKubernetesObject)}"),
        };
}
