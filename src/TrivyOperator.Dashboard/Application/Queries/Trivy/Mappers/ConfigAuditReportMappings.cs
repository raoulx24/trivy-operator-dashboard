using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ConfigAuditReportExtensions
{
    public static ConfigAuditReportDto ToDto(
        this ConfigAuditReport report)
    {
        SecurityAssessmentReportDetailDto[] details =
        [
            .. report.Checks.Select(x => x.ToDto()),
        ];

        return new ConfigAuditReportDto(
            report.Metadata.Uid.Value,
            report.Metadata.GetResourceName().Value,
            report.Metadata.NamespaceName.Value,
            report.Metadata.GetResourceKind().Value,
            report.SeverityCounters.CriticalCount,
            report.SeverityCounters.HighCount,
            report.SeverityCounters.MediumCount,
            report.SeverityCounters.LowCount,
            report.LastSeenAt.Value,
            details
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
                uid,
                resourceName,
                resourceNamespace,
                resourceKind,
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
