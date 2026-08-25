using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.SecurityAssessments;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ClusterInfraAssessmentReportMappings
{
    public static ClusterInfraAssessmentReportDto ToDto(
        this ClusterInfraAssessmentReport report)
    {
        return new ClusterInfraAssessmentReportDto(
            Uid: report.Metadata.Uid.Value,
            ResourceName: report.Resource.Name.Value,
            ResourceKind: report.Resource.Kind.Value,
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

    public static IEnumerable<ClusterInfraAssessmentReportDenormalizedDto> ToDenormalizedDtos(
        this ClusterInfraAssessmentReport report)
    {
        return report.Checks.Select(check => new ClusterInfraAssessmentReportDenormalizedDto(
            Uid: report.Metadata.Uid.Value,
            ResourceName: report.Resource.Name.Value,
            ResourceKind: report.Resource.Kind.Value,
            Category: check.Category.Value,
            CheckId: check.CheckId.Value,
            Description: check.Description.Value,
            Messages: check.Messages,
            Remediation: check.Remediation.Value,
            SeverityId: check.Severity.Rank,
            Success: check.Success,
            Title: check.Title.Value
        ));
    }
}
