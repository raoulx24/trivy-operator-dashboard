using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ConfigAuditReportExtensions
{
    public static ConfigAuditReportDto ToDto(
        this ConfigAuditReport report)
    {
        return new ConfigAuditReportDto(
            Uid: report.Metadata.Uid.Value,
            ResourceName: report.Metadata.GetResourceName().Value,
            ResourceNamespace: report.Metadata.NamespaceName.Value,
            ResourceKind: report.Metadata.GetResourceKind().Value,
            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            UpdateTimestamp: report.LastSeenAt.Value,
            Details: [.. report.Checks.Select(x => x.ToDto()),]
        );
    }

    public static IEnumerable<ConfigAuditReportDenormalizedDto> ToDenormalizedDtos(
        this ConfigAuditReport report)
    {
        string uid = report.Metadata.Uid.Value;
        string resourceName = report.Metadata.GetResourceName().Value;
        string resourceNamespace = report.Metadata.NamespaceName.Value;
        string resourceKind = report.Metadata.GetResourceKind().Value;

        return report.Checks.Select(check =>
        {
            SecurityAssessmentReportDetailDto detail = check.ToDto();

            return new ConfigAuditReportDenormalizedDto(
                Uid: uid,
                ResourceName: resourceName,
                ResourceNamespace: resourceNamespace,
                ResourceKind: resourceKind,
                UpdateTimestamp: report.LastSeenAt.Value,
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
