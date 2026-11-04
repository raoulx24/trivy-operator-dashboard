using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;

public static class ClusterRbacAssessmentReportMappings
{
    public static ClusterRbacAssessmentReportDto ToDto(
        this ClusterRbacAssessmentReport report)
    {
        return new ClusterRbacAssessmentReportDto(
            Uid: report.Metadata.Uid.ToString(),
            ResourceName: report.Metadata.GetResourceName().Value,
            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UpdateTimestamp: report.LastSeenAt.Value,
            Details: [.. report.Checks.ToDetailDtos(),]
        );
    }

    public static IEnumerable<ClusterRbacAssessmentReportDenormalizedDto> ToDenormalizedDtos(
        this ClusterRbacAssessmentReport report)
    {
        return report.Checks.ToDetailDtos().Select(detail =>
            new ClusterRbacAssessmentReportDenormalizedDto(
                Uid: report.Metadata.Uid.ToString(),
                ResourceName: report.Metadata.GetResourceName().Value,
                CriticalCount: report.SeverityCounters.CriticalCount,
                HighCount: report.SeverityCounters.HighCount,
                MediumCount: report.SeverityCounters.MediumCount,
                LowCount: report.SeverityCounters.LowCount,
                UpdateTimestamp: report.LastSeenAt.Value,
                Category: detail.Category,
                CheckId: detail.CheckId,
                Description: detail.Description,
                Remediation: detail.Remediation,
                SeverityId: detail.SeverityId,
                Title: detail.Title,
                Count: detail.Count
            )
        );
    }
}
