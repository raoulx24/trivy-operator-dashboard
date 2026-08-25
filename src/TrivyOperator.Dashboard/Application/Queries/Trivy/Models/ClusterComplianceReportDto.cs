namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record ClusterComplianceReportDto(
    string Name,
    string Uid,

    string Description,
    string Platform,
    IReadOnlyList<string> RelatedResources,
    string Title,
    string Type,
    string Version,

    string Cron,
    string ReportType,

    int TotalPassCount,
    int TotalFailCount,
    int TotalFailCriticalCount,
    int TotalFailHighCount,
    int TotalFailMediumCount,
    int TotalFailLowCount,

    DateTime? UpdateTimestamp,

    IReadOnlyList<ClusterComplianceReportDetailDto> Details
);

public sealed record ClusterComplianceReportDetailDto(
    string Id,
    string Name,
    string Description,
    int SeverityId,
    IReadOnlyList<string> Checks,
    IReadOnlyList<string> Commands,
    int TotalFail
);

public sealed record ClusterComplianceReportDenormalizedDto(
    string Name,
    string Uid,

    string Description,
    string Platform,
    IReadOnlyList<string> RelatedResources,
    string Title,
    string Type,
    string Version,

    string Cron,
    string ReportType,

    int TotalPassCount,
    int TotalFailCount,

    DateTime? UpdateTimestamp,

    string DetailId,
    string DetailName,
    string DetailDescription,
    int SeverityId,
    IReadOnlyList<string> Checks,
    IReadOnlyList<string> Commands,
    int TotalFail
);
