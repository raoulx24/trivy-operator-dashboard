using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ClusterRbacAssessmentReportMappings
{
    public static ClusterRbacAssessmentReportDto ToDto(
        this ClusterRbacAssessmentReport report)
    {
        SecurityAssessmentReportDetailDto[] details =
        [
            .. report.Checks.Select(x => x.ToDto()),
        ];

        return new ClusterRbacAssessmentReportDto(
            Uid: report.Metadata.Uid.ToString(),
            ResourceName: report.Metadata.GetResourceName().Value,
            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UpdateTimestamp: report.LastSeenAt.Value,
            Details: details
        );
    }

    public static IEnumerable<ClusterRbacAssessmentReportDenormalizedDto> ToDenormalizedDtos(
        this ClusterRbacAssessmentReport report)
    {
        return report.Checks.Select(controlCheck =>
        {
            SecurityAssessmentReportDetailDto detail = controlCheck.ToDto();

            return new ClusterRbacAssessmentReportDenormalizedDto(
                Uid: report.Metadata.Uid.ToString(),
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
