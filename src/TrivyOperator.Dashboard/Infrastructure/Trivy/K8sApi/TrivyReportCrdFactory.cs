using TrivyOperator.Dashboard.Infrastructure.K8s.CustomResources;
using TrivyOperator.Dashboard.Infrastructure.K8s.Services.Abstractions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Crds;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

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
