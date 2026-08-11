
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Factories;

public static class TrivyReportFactory
{
    public static T CreateSecurityAssessment<T>(
        ReportMetadata metadata,
        Resource resource,
        Scanner scanner,
        SeverityCounters severityCounters,
        Timestamp updateTimestamp,
        IReadOnlyList<Check> checks)
        where T : ITrivyReport
    {
        return typeof(T) switch
        {
            var t when t == typeof(ClusterConfigAuditReport)
                => (T)(ITrivyReport)new ClusterConfigAuditReport(metadata, resource, scanner, severityCounters, updateTimestamp, checks),
            var t when t == typeof(ClusterInfraAssessmentReport)
                => (T)(ITrivyReport)new ClusterInfraAssessmentReport(metadata, resource, scanner, severityCounters, updateTimestamp, checks),
            var t when t == typeof(ClusterRbacAssessmentReport)
                => (T)(ITrivyReport)new ClusterRbacAssessmentReport(metadata, resource, scanner, severityCounters, updateTimestamp, checks),
            
            var t when t == typeof(ConfigAuditReport)
                => (T)(ITrivyReport)new ConfigAuditReport(metadata, resource, scanner, severityCounters, updateTimestamp, checks),
            var t when t == typeof(InfraAssessmentReport)
                => (T)(ITrivyReport)new InfraAssessmentReport(metadata, resource, scanner, severityCounters, updateTimestamp, checks),
            var t when t == typeof(RbacAssessmentReport)
                => (T)(ITrivyReport)new RbacAssessmentReport(metadata, resource, scanner, severityCounters, updateTimestamp, checks),

            _ => throw new InvalidOperationException($"Unsupported type {typeof(T)}"),
        };
    }
}
