using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;

public static class ClusterConfigAuditReportExtensions
{
    public static ClusterConfigAuditReportDto ToClusterDto(
        this ClusterConfigAuditReport report)
    {
        return new ClusterConfigAuditReportDto(
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

    public static IEnumerable<ClusterConfigAuditReportDenormalizedDto> ToClusterDenormalizedDtos(
            this ClusterConfigAuditReport report)
    {
        return report.Checks.ToDetailDtos().Select(detail =>
            new ClusterConfigAuditReportDenormalizedDto(
                Uid: report.Metadata.Uid.Value,
                ResourceName: report.Metadata.GetResourceName().Value,
                ResourceKind: report.Metadata.GetResourceKind().Value,
                UpdateTimestamp: report.LastSeenAt.Value,
                CriticalCount: report.SeverityCounters.CriticalCount,
                HighCount: report.SeverityCounters.HighCount,
                MediumCount: report.SeverityCounters.MediumCount,
                LowCount: report.SeverityCounters.LowCount,
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
