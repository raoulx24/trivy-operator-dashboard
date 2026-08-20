using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ExposedSecretReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.RbacAssessmentReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.VulnerabilityReports.Models;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Factories;

public class TrivyReportCrTypeFactory
{
    private static readonly Dictionary<string, Type> Types = new(StringComparer.Ordinal)
    {
        [nameof(ClusterComplianceReportCr)] = typeof(ClusterComplianceReportCr),
        [nameof(ClusterConfigAuditReportCr)] = typeof(ClusterConfigAuditReportCr),
        [nameof(ConfigAuditReportCr)] = typeof(ConfigAuditReportCr),
        [nameof(ClusterInfraAssessmentReportCr)] = typeof(ClusterInfraAssessmentReportCr),
        [nameof(InfraAssessmentReportCr)] = typeof(InfraAssessmentReportCr),
        [nameof(ClusterRbacAssessmentReportCr)] = typeof(ClusterRbacAssessmentReportCr),
        [nameof(RbacAssessmentReportCr)] = typeof(RbacAssessmentReportCr),
        [nameof(ExposedSecretReportCr)] = typeof(ExposedSecretReportCr),
        [nameof(ClusterSbomReportCr)] = typeof(ClusterSbomReportCr),
        [nameof(SbomReportCr)] = typeof(SbomReportCr),
        [nameof(ClusterVulnerabilityReportCr)] = typeof(ClusterVulnerabilityReportCr),
        [nameof(VulnerabilityReportCr)] = typeof(VulnerabilityReportCr),
    };

    public static Type Get(string name) =>
        Types.TryGetValue(name, out Type? type)
            ? type
            : throw new ArgumentOutOfRangeException(
                nameof(name),
                name,
                $"Unknown Trivy report CR type: '{name}'.");
}
