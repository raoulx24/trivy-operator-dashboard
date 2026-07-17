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
using TrivyOperator.Dashboard.Domain.TrivyOld.VulnerabilityReport;
using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.K8sApi;

public class TrivyReportCrdFactory : ICrdFactory
{
    public CustomResourceDefinition Get<TKubernetesObject>() => typeof(TKubernetesObject) switch
    {
        { } t when t == typeof(OldClusterComplianceReportCr) => new ClusterComplianceReportCrd(),
        { } t when t == typeof(OldClusterInfraAssessmentReportCr) => new ClusterInfraAssessmentReportCrd(),
        { } t when t == typeof(OldClusterRbacAssessmentReportCr) => new ClusterRbacAssessmentReportCrd(),
        { } t when t == typeof(OldClusterSbomReportCr) => new ClusterSbomReportCrd(),
        { } t when t == typeof(OldClusterVulnerabilityReportCr) => new ClusterVulnerabilityReportCrd(),
        { } t when t == typeof(OldConfigAuditReportCr) => new ConfigAuditReportCrd(),
        { } t when t == typeof(OldExposedSecretReportCr) => new ExposedSecretReportCrd(),
        { } t when t == typeof(OldInfraAssessmentReportCr) => new InfraAssessmentReportCrd(),
        { } t when t == typeof(OldRbacAssessmentReportCr) => new RbacAssessmentReportCrd(),
        { } t when t == typeof(OldSbomReportCr) => new SbomReportCrd(),
        { } t when t == typeof(OldVulnerabilityReportCr) => new VulnerabilityReportCrd(),
        { } t when t == typeof(VulnerabilityReportCr) => new VulnerabilityReportCrd(),
        _ => throw new InvalidOperationException($"Unsupported Kubernetes object type - {typeof(TKubernetesObject)}"),
    };
}
