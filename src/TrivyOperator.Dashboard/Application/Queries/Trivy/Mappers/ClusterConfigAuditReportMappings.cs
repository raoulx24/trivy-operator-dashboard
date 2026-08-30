using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ClusterConfigAuditReportExtensions
{
    public static ClusterConfigAuditReportDto ToClusterDto(
        this ConfigAuditReport report)
    {
        SecurityAssessmentReportDetailDto[] details =
        [
            .. report.Checks.Select(static x => x.ToDto()),
        ];

        return new ClusterConfigAuditReportDto(
            report.Metadata.Uid.Value,
            report.Metadata.GetResourceName().Value,
            report.SeverityCounters.CriticalCount,
            report.SeverityCounters.HighCount,
            report.SeverityCounters.MediumCount,
            report.SeverityCounters.LowCount,
            report.LastSeenAt.Value,
            details
        );
    }

    public static IEnumerable<ClusterConfigAuditReportDenormalizedDto>
        ToClusterDenormalizedDtos(
            this ConfigAuditReport report)
    {
        string uid = report.Metadata.Uid.Value;
        string resourceName = report.Metadata.GetResourceName().Value;

        return report.Checks.Select(check =>
        {
            SecurityAssessmentReportDetailDto detail = check.ToDto();

            return new ClusterConfigAuditReportDenormalizedDto(
                uid,
                resourceName,
                report.SeverityCounters.CriticalCount,
                report.SeverityCounters.HighCount,
                report.SeverityCounters.MediumCount,
                report.SeverityCounters.LowCount,
                detail.Category,
                detail.CheckId,
                detail.Description,
                detail.Messages,
                detail.Remediation,
                detail.SeverityId,
                detail.Success,
                detail.Title
            );
        });
    }
}
