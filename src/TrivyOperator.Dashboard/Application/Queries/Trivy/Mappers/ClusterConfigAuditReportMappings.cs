using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ClusterConfigAuditReportExtensions
{
    public static ClusterConfigAuditReportDto ToClusterDto(
        this ConfigAuditReport report)
    {
        return new ClusterConfigAuditReportDto(
            Uid: report.Metadata.Uid.Value,
            ResourceName: report.Metadata.GetResourceName().Value,
            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UpdateTimestamp: report.LastSeenAt.Value,
            Details: [.. report.Checks.Select(static x => x.ToDto()),]
        );
    }

    public static IEnumerable<ClusterConfigAuditReportDenormalizedDto> ToClusterDenormalizedDtos(
            this ConfigAuditReport report)
    {
        return report.Checks.Select(check =>
        {
            SecurityAssessmentReportDetailDto detail = check.ToDto();

            return new ClusterConfigAuditReportDenormalizedDto(
                Uid: report.Metadata.Uid.Value,
                ResourceName: report.Metadata.GetResourceName().Value,
                CriticalCount: report.SeverityCounters.CriticalCount,
                HighCount: report.SeverityCounters.HighCount,
                MediumCount: report.SeverityCounters.MediumCount,
                LowCount: report.SeverityCounters.LowCount,
                Category: detail.Category,
                CheckId: detail.CheckId,
                Description: detail.Description,
                Messages: detail.Messages,
                Remediation: detail.Remediation,
                SeverityId: detail.SeverityId,
                Success: detail.Success,
                Title: detail.Title
            );
        });
    }
}
