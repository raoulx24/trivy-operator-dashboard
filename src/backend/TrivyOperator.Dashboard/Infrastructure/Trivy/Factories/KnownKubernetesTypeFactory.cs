using k8s.Models;
using TrivyOperator.Dashboard.Application.Kubernetes.WatcherState.Abstractions;
using TrivyOperator.Dashboard.Domain.Kubernetes.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Factories;

// maps the domain type name to the watched kubernetes object type
public class KnownKubernetesTypeFactory : IKnownKubernetesTypeFactory
{
    private static readonly Dictionary<string, Type> Types = new(StringComparer.Ordinal)
    {
        // kubernetes types
        [nameof(KubernetesNamespace)] = typeof(V1Namespace),

        // trivy types
        [nameof(ClusterComplianceReport)] = typeof(ClusterComplianceReportCr),
        [nameof(ClusterConfigAuditReport)] = typeof(ClusterConfigAuditReportCr),
        [nameof(ConfigAuditReport)] = typeof(ConfigAuditReportCr),
        [nameof(ClusterInfraAssessmentReport)] = typeof(ClusterInfraAssessmentReportCr),
        [nameof(InfraAssessmentReport)] = typeof(InfraAssessmentReportCr),
        [nameof(ClusterRbacAssessmentReport)] = typeof(ClusterRbacAssessmentReportCr),
        [nameof(RbacAssessmentReport)] = typeof(RbacAssessmentReportCr),
        [nameof(ExposedSecretReport)] = typeof(ExposedSecretReportCr),
        [nameof(ClusterSbomReport)] = typeof(ClusterSbomReportCr),
        [nameof(SbomReport)] = typeof(SbomReportCr),
        [nameof(ClusterVulnerabilityReport)] = typeof(ClusterVulnerabilityReportCr),
        [nameof(VulnerabilityReport)] = typeof(VulnerabilityReportCr),
    };

    public Type Get(string name) =>
        Types.TryGetValue(name, out Type? type)
            ? type
            : throw new ArgumentOutOfRangeException(
                nameof(name),
                name,
                $"Unknown watched resource type: '{name}'.");
}
