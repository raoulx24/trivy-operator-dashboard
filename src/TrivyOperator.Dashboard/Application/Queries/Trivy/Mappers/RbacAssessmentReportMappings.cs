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
            report.Metadata.Uid.ToString(),
            report.Resource.GetResourceName(),
            report.Resource.NamespaceName.ToString(),
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
        string uid = report.Metadata.Uid.ToString();
        string resourceName = report.Resource.GetResourceName();
        string resourceNamespace = report.Resource.NamespaceName.ToString();

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
