using Microsoft.Extensions.Options;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterComplianceReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.InfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.RbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Options;
using TrivyOperator.Dashboard.Domain.TrivyOld.VulnerabilityReport;

namespace TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository;

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
