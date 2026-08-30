using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class RbacAssessmentReportExtensions
{
    public static RbacAssessmentReportDto ToDto(
        this RbacAssessmentReport report)
    {
        SecurityAssessmentReportDetailDto[] details =
        [
            .. report.Checks.Select(x => x.ToDto()),
        ];

        return new RbacAssessmentReportDto(
            report.Metadata.Uid.Value,
            report.Metadata.GetResourceName().Value,
            report.Metadata.NamespaceName.Value,
            report.SeverityCounters.CriticalCount,
            report.SeverityCounters.HighCount,
            report.SeverityCounters.MediumCount,
            report.SeverityCounters.LowCount,
            report.Metadata.CreationTimestamp.Value,
            details
        );
    }

    public static IEnumerable<RbacAssessmentReportDenormalizedDto> ToDenormalizedDtos(
        this RbacAssessmentReport report)
    {
        string uid = report.Metadata.Uid.Value;
        string resourceName = report.Metadata.GetResourceName().Value;
        string resourceNamespace = report.Metadata.NamespaceName.Value;

        return report.Checks.Select(check =>
        {
            SecurityAssessmentReportDetailDto detail = check.ToDto();

            return new RbacAssessmentReportDenormalizedDto(
                uid,
                resourceName,
                resourceNamespace,
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
