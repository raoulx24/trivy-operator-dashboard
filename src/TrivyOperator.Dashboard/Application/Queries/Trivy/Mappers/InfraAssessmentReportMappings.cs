using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class InfraAssessmentReportMappings
{
    public static InfraAssessmentReportDto ToDto(
        this InfraAssessmentReport report)
    {
        return new InfraAssessmentReportDto(
            Uid: report.Metadata.Uid.Value,
            ResourceName: report.Metadata.Name.Value,
            ResourceNamespace: report.Metadata.NamespaceName.Value,
            CriticalCount: report.SeverityCounters.CriticalCount,
            HighCount: report.SeverityCounters.HighCount,
            MediumCount: report.SeverityCounters.MediumCount,
            LowCount: report.SeverityCounters.LowCount,
            Details:
            [
                .. report.Checks.Select(static x => x.ToDto())
            ]
        );
    }

    public static IEnumerable<InfraAssessmentReportDenormalizedDto> ToDenormalizedDtos(
        this InfraAssessmentReport report)
    {
        return report.Checks.Select(check =>
            new InfraAssessmentReportDenormalizedDto(
                Uid: report.Metadata.Uid.Value,
                ResourceName: report.Metadata.Name.Value,
                ResourceNamespace: report.Metadata.NamespaceName.Value,
                Category: check.Category.Value,
                CheckId: check.CheckId.Value,
                Description: check.Description.Value,
                Messages: check.Messages,
                Remediation: check.Remediation.Value,
                SeverityId: check.Severity.Rank,
                Success: check.Success,
                Title: check.Title.Value
            )
        );
    }
}