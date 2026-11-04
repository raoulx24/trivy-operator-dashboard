using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;

public static class ClusterInfraAssessmentReportMappings
{
    public static ClusterInfraAssessmentReportDto ToDto(
        this ClusterInfraAssessmentReport report)
    {
        return new ClusterInfraAssessmentReportDto(
            Uid: report.Metadata.Uid.Value,
            ResourceName: report.Metadata.GetResourceName().Value,
            ResourceKind: report.Metadata.GetResourceKind().Value,
            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UpdateTimestamp: report.LastSeenAt.Value,
            Details: [.. report.Checks.ToDetailDtos(),]
        );
    }

    public static IEnumerable<ClusterInfraAssessmentReportDenormalizedDto> ToDenormalizedDtos(
        this ClusterInfraAssessmentReport report)
    {
        return report.Checks.ToDetailDtos().Select(detail => new ClusterInfraAssessmentReportDenormalizedDto(
            Uid: report.Metadata.Uid.Value,
            ResourceName: report.Metadata.GetResourceName().Value,
            ResourceKind: report.Metadata.GetResourceKind().Value,
            UpdateTimestamp: report.LastSeenAt.Value,
            Category: detail.Category,
            CheckId: detail.CheckId,
            Description: detail.Description,
            Remediation: detail.Remediation,
            SeverityId: detail.SeverityId,
            Title: detail.Title,
            Count: detail.Count
        ));
    }
}
