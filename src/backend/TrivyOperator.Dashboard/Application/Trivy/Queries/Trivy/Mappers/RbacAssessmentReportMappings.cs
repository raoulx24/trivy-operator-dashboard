using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;

public static class RbacAssessmentReportMappings
{
    public static RbacAssessmentReportDto ToDto(
        this RbacAssessmentReport report)
    {
        return new RbacAssessmentReportDto(
            Uid: report.Metadata.Uid.Value,
            ResourceName: report.Metadata.GetResourceName().Value,
            ResourceNamespace: report.Metadata.NamespaceName.Value,
            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UpdateTimestamp: report.Metadata.CreationTimestamp.Value,
            Details: [.. report.Checks.ToDetailDtos(),]
        );
    }

    public static IEnumerable<RbacAssessmentReportDenormalizedDto> ToDenormalizedDtos(
        this RbacAssessmentReport report)
    {
        return report.Checks.ToDetailDtos().Select(detail =>
            new RbacAssessmentReportDenormalizedDto(
                Uid: report.Metadata.Uid.Value,
                ResourceName: report.Metadata.GetResourceName().Value,
                ResourceNamespace: report.Metadata.NamespaceName.Value,
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
