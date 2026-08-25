using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.ClusterCompliance;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ClusterComplianceReportMappings
{
    public static ClusterComplianceReportDto ToDto(
        this ClusterComplianceReport report)
    {
        ClusterComplianceReportDetailDto[] details =
        [
            .. report.ControlChecks.Select(ToDto),
        ];

        return new ClusterComplianceReportDto(
            Name: report.Metadata.Name.Value,
            Uid: report.Metadata.Uid.Value,

            Description: report.ComplianceMetadata.Description.Value,
            Platform: report.ComplianceMetadata.CompliancePlatform.Value,
            RelatedResources:
            [
                .. report.ComplianceMetadata.RelatedResources
                    .Select(x => x.InitialValue),
            ],
            Title: report.ComplianceMetadata.Title.Value,
            Type: report.ComplianceMetadata.Type.Value,
            Version: report.ComplianceMetadata.Version.Value,

            Cron: report.Schedule.Value,
            ReportType: report.ComplianceMetadata.Id.Value,

            TotalPassCount: report.Summary.PassCount,
            TotalFailCount: report.Summary.FailCount,
            TotalFailCriticalCount: CountFailed(details, 0),
            TotalFailHighCount: CountFailed(details, 1),
            TotalFailMediumCount: CountFailed(details, 2),
            TotalFailLowCount: CountFailed(details, 3),

            UpdateTimestamp: report.LastSeenAt.Value,

            Details: details
        );
    }

    public static IEnumerable<ClusterComplianceReportDenormalizedDto> ToDenormalizedDtos(
        this ClusterComplianceReport report)
    {
        return report.ControlChecks.Select(controlCheck =>
            new ClusterComplianceReportDenormalizedDto(
                Name: report.Metadata.Name.Value,
                Uid: report.Metadata.Uid.Value,

                Description: report.ComplianceMetadata.Description.Value,
                Platform: report.ComplianceMetadata.CompliancePlatform.Value,
                RelatedResources:
                [
                    .. report.ComplianceMetadata.RelatedResources
                        .Select(x => x.InitialValue),
                ],
                Title: report.ComplianceMetadata.Title.Value,
                Type: report.ComplianceMetadata.Type.Value,
                Version: report.ComplianceMetadata.Version.Value,

                Cron: report.Schedule.Value,
                ReportType: report.ComplianceMetadata.Id.Value,

                TotalPassCount: report.Summary.PassCount,
                TotalFailCount: report.Summary.FailCount,

                UpdateTimestamp: report.LastSeenAt.Value,

                DetailId: controlCheck.Control.Id.Value,
                DetailName: controlCheck.Control.ControlName.Value,
                DetailDescription: controlCheck.Control.Description.Value,
                SeverityId: controlCheck.Control.Severity.Rank,
                Checks: [.. controlCheck.Control.Checks.Select(x => x.Value),],
                Commands: [.. controlCheck.Control.Commands.Select(x => x.Value),],
                TotalFail: controlCheck.TotalFail.Value
            ));
    }

    private static ClusterComplianceReportDetailDto ToDto(
        ControlResult controlCheck)
    {
        return new ClusterComplianceReportDetailDto(
            Id: controlCheck.Control.Id.Value,
            Name: controlCheck.Control.ControlName.Value,
            Description: controlCheck.Control.Description.Value,
            SeverityId: controlCheck.Control.Severity.Rank,
            Checks: [.. controlCheck.Control.Checks.Select(x => x.Value),],
            Commands: [.. controlCheck.Control.Commands.Select(x => x.Value),],
            TotalFail: controlCheck.TotalFail.Value
        );
    }

    private static int CountFailed(
        IEnumerable<ClusterComplianceReportDetailDto> details,
        int severityRank)
        => details.Count(x =>
            x.SeverityId == severityRank &&
            x.TotalFail > 0);
}
