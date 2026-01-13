using TrivyOperator.Dashboard.Domain.Trivy.ClusterComplianceReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterVulnerabilityReport;
using TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.Trivy.CustomResources.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Trivy.InfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.RbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
using TrivyOperator.Dashboard.Domain.Trivy.Services.K8s.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.VulnerabilityReport;

namespace TrivyOperator.Dashboard.Domain.Trivy.Services.K8s;

public class CustomResourceDefinitionFactory : ICustomResourceDefinitionFactory
{
    public CustomResourceDefinition Get<TKubernetesObject>()
        => typeof(TKubernetesObject) switch
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
