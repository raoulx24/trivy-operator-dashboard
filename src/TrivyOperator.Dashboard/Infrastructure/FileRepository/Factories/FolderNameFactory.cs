using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.FileRepository.Options;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Factories;

public class FolderNameFactory(IOptions<FileRepositoryOptions> options) : IFolderNameFactory
{
    public string Get<TKubernetesObject>() => typeof(TKubernetesObject) switch
    {
        { } t when t == typeof(ClusterComplianceReportCr) => options.Value.ClusterComplianceReportCrSubpath,

        { } t when t == typeof(ClusterInfraAssessmentReportCr) => options.Value.ClusterInfraAssessmentReportCrSubpath,

        { } t when t == typeof(ClusterRbacAssessmentReportCr) => options.Value.ClusterRbacAssessmentReportCrSubpath,

        { } t when t == typeof(ClusterSbomReportCr) => options.Value.ClusterSbomReportCrSubpath,

        { } t when t == typeof(ClusterVulnerabilityReportCr) => options.Value.ClusterVulnerabilityReportCrSubpath,

        { } t when t == typeof(ConfigAuditReportCr) => options.Value.ConfigAuditReportCrSubpath,

        { } t when t == typeof(ExposedSecretReportCr) => options.Value.ExposedSecretReportCrSubpath,

        { } t when t == typeof(InfraAssessmentReportCr) => options.Value.InfraAssessmentReportCrSubpath,

        { } t when t == typeof(RbacAssessmentReportCr) => options.Value.RbacAssessmentReportCrSubpath,

        { } t when t == typeof(SbomReportCr) => options.Value.SbomReportCrSubpath,

        { } t when t == typeof(VulnerabilityReportCr) => options.Value.VulnerabilityReportCrSubpath,

        _ => throw new InvalidOperationException($"Unsupported Kubernetes object type - {typeof(TKubernetesObject)}"),
    };
}
