using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterComplianceReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.TrivyOld.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.InfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.RbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;
using TrivyOperator.Dashboard.Domain.TrivyOld.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.K8sApi;

public class TrivyReportCrdFactory : ICrdFactory
{
    public CustomResourceDefinition Get<TKubernetesObject>() => typeof(TKubernetesObject) switch
    {
        { } t when t == typeof(ClusterComplianceReportCr) => new ClusterComplianceReportCrd(),
        { } t when t == typeof(ClusterInfraAssessmentReportCr) => new ClusterInfraAssessmentReportCrd(),
        { } t when t == typeof(ClusterRbacAssessmentReportCr) => new ClusterRbacAssessmentReportCrd(),
        { } t when t == typeof(ClusterSbomReportCr) => new ClusterSbomReportCrd(),
        { } t when t == typeof(ClusterVulnerabilityReportCr) => new ClusterVulnerabilityReportCrd(),
        { } t when t == typeof(ConfigAuditReportCr) => new ConfigAuditReportCrd(),
        { } t when t == typeof(ExposedSecretReportCr) => new ExposedSecretReportCrd(),
        { } t when t == typeof(InfraAssessmentReportCr) => new InfraAssessmentReportCrd(),
        { } t when t == typeof(RbacAssessmentReportCr) => new RbacAssessmentReportCrd(),
        { } t when t == typeof(SbomReportCr) => new SbomReportCrd(),
        { } t when t == typeof(VulnerabilityReportCr) => new VulnerabilityReportCrd(),
        _ => throw new InvalidOperationException($"Unsupported Kubernetes object type - {typeof(TKubernetesObject)}"),
    };
}
