using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.TrivyOld;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterComplianceReport;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class ClusterComplianceReportDto
{
    public string Name { get; init; } = string.Empty;
    public Guid Uid { get; init; } = Guid.NewGuid();
    public string Description { get; init; } = string.Empty;
    public string Platform { get; init; } = string.Empty;
    public string[] RelatedResources { get; init; } = [];
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Cron { get; init; } = string.Empty;
    public string ReportType { get; init; } = string.Empty;
    public long TotalPassCount { get; init; }
    public long TotalFailCount { get; init; }
    public long TotalFailCriticalCount { get; init; }
    public long TotalFailHighCount { get; init; }
    public long TotalFailMediumCount { get; init; }
    public long TotalFailLowCount { get; init; }
    public DateTime? UpdateTimestamp { get; init; }
    public ClusterComplianceReportDetailDto[] Details { get; set; } = [];
}

public class ClusterComplianceReportDetailDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public string[] Checks { get; set; } = [];
    public string[] Commands { get; set; } = [];
    public long TotalFail { get; init; }
}

public class ClusterComplianceReportDenormalizedDto
{
    public string Name { get; init; } = string.Empty;
    public Guid Uid { get; init; } = Guid.NewGuid();
    public string Description { get; init; } = string.Empty;
    public string Platform { get; init; } = string.Empty;
    public string[] RelatedResources { get; init; } = [];
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
    public string Cron { get; init; } = string.Empty;
    public string ReportType { get; init; } = string.Empty;
    public long TotalPassCount { get; init; }
    public long TotalFailCount { get; init; }
    public DateTime? UpdateTimestamp { get; init; }

    public string DetailId { get; init; } = string.Empty;
    public string DetailName { get; init; } = string.Empty;
    public string DetailDescription { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public string[] Checks { get; set; } = [];
    public string[] Commands { get; set; } = [];
    public long TotalFail { get; init; }
}

public static class ClusterComplianceReportCrExtensions
{
    public static ClusterComplianceReportDto ToClusterComplianceReportDto(
        this OldClusterComplianceReportCr oldClusterComplianceReportCr
    )
    {
        ClusterComplianceReportDetailDto[] details =
        [
            .. oldClusterComplianceReportCr.Spec.Compliance.Controls.Select(control => new ClusterComplianceReportDetailDto
                {
                    Id = control.Id,
                    Name = control.Name,
                    Description = control.Description,
                    SeverityId = (int)control.Severity,
                    Checks = control.Checks.Select(check => check.Id).ToArray(),
                    Commands = control.Commands.Select(command => command.Id).ToArray(),
                    TotalFail = oldClusterComplianceReportCr.Status.SummaryReport.ControlCheck
                                    .Where(x => x.Id == control.Id)
                                    .FirstOrDefault()
                                    ?.TotalFail ??
                                0,
                }
            ),
        ];

        return new ClusterComplianceReportDto
        {
            Name = oldClusterComplianceReportCr.Metadata.Name,
            Uid = Guid.TryParse(oldClusterComplianceReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            Description = oldClusterComplianceReportCr.Spec.Compliance.Description,
            Platform = oldClusterComplianceReportCr.Spec.Compliance.Platform,
            RelatedResources =
                oldClusterComplianceReportCr.Spec.Compliance.RelatedResources.Select(x => x.ToString()).ToArray(),
            Title = oldClusterComplianceReportCr.Spec.Compliance.Title,
            Type = oldClusterComplianceReportCr.Spec.Compliance.Type,
            Version = oldClusterComplianceReportCr.Spec.Compliance.Version,
            Cron = oldClusterComplianceReportCr.Spec.Cron,
            ReportType = oldClusterComplianceReportCr.Spec.ReportType,
            TotalPassCount = oldClusterComplianceReportCr.Status.Summary.PassCount,
            TotalFailCount = oldClusterComplianceReportCr.Status.Summary.FailCount,
            TotalFailCriticalCount =
                details.Where(x => x.SeverityId == (int)TrivySeverity.CRITICAL && x.TotalFail > 0).Count(),
            TotalFailHighCount = details.Where(x => x.SeverityId == (int)TrivySeverity.HIGH && x.TotalFail > 0).Count(),
            TotalFailMediumCount =
                details.Where(x => x.SeverityId == (int)TrivySeverity.MEDIUM && x.TotalFail > 0).Count(),
            TotalFailLowCount = details.Where(x => x.SeverityId == (int)TrivySeverity.LOW && x.TotalFail > 0).Count(),
            UpdateTimestamp = oldClusterComplianceReportCr.Status.UpdateTimestamp,
            Details = details,
        };
    }

    public static IEnumerable<ClusterComplianceReportDenormalizedDto> ToClusterComplianceReportDenormalizedDtos(
        this OldClusterComplianceReportCr oldClusterComplianceReportCr
    ) =>
        oldClusterComplianceReportCr.Spec.Compliance.Controls.Select(control => new ClusterComplianceReportDenormalizedDto
            {
                Name = oldClusterComplianceReportCr.Metadata.Name,
                Uid = new Guid(oldClusterComplianceReportCr.Metadata.Uid),
                Description = oldClusterComplianceReportCr.Spec.Compliance.Description,
                Platform = oldClusterComplianceReportCr.Spec.Compliance.Platform,
                RelatedResources =
                    oldClusterComplianceReportCr.Spec.Compliance.RelatedResources.Select(x => x.ToString()).ToArray(),
                Title = oldClusterComplianceReportCr.Spec.Compliance.Title,
                Type = oldClusterComplianceReportCr.Spec.Compliance.Type,
                Version = oldClusterComplianceReportCr.Spec.Compliance.Version,
                Cron = oldClusterComplianceReportCr.Spec.Cron,
                ReportType = oldClusterComplianceReportCr.Spec.ReportType,
                TotalPassCount = oldClusterComplianceReportCr.Status.Summary.PassCount,
                TotalFailCount = oldClusterComplianceReportCr.Status.Summary.FailCount,
                UpdateTimestamp = oldClusterComplianceReportCr.Status.UpdateTimestamp,
                DetailId = control.Id,
                DetailName = control.Name,
                DetailDescription = control.Description,
                SeverityId = (int)control.Severity,
                Checks = control.Checks.Select(check => check.Id).ToArray(),
                Commands = control.Commands.Select(command => command.Id).ToArray(),
                TotalFail = oldClusterComplianceReportCr.Status.SummaryReport.ControlCheck.Where(x => x.Id == control.Id)
                                .FirstOrDefault()
                                ?.TotalFail ??
                            0,
            }
        );
}
