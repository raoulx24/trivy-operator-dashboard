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
        { } t when t == typeof(OldClusterComplianceReportCr) => options.Value.ClusterComplianceReportCrSubpath,

        { } t when t == typeof(OldClusterInfraAssessmentReportCr) => options.Value.ClusterInfraAssessmentReportCrSubpath,

        { } t when t == typeof(OldClusterRbacAssessmentReportCr) => options.Value.ClusterRbacAssessmentReportCrSubpath,

        { } t when t == typeof(OldClusterSbomReportCr) => options.Value.ClusterSbomReportCrSubpath,

        { } t when t == typeof(OldClusterVulnerabilityReportCr) => options.Value.ClusterVulnerabilityReportCrSubpath,

        { } t when t == typeof(OldConfigAuditReportCr) => options.Value.ConfigAuditReportCrSubpath,

        { } t when t == typeof(OldExposedSecretReportCr) => options.Value.ExposedSecretReportCrSubpath,

        { } t when t == typeof(OldInfraAssessmentReportCr) => options.Value.InfraAssessmentReportCrSubpath,

        { } t when t == typeof(OldRbacAssessmentReportCr) => options.Value.RbacAssessmentReportCrSubpath,

        { } t when t == typeof(OldSbomReportCr) => options.Value.SbomReportCrSubpath,

        { } t when t == typeof(OldVulnerabilityReportCr) => options.Value.VulnerabilityReportCrSubpath,

        _ => throw new InvalidOperationException($"Unsupported Kubernetes object type - {typeof(TKubernetesObject)}"),
    };
}
