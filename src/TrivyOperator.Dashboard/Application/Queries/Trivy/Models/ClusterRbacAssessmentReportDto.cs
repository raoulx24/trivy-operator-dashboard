namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record ClusterRbacAssessmentReportDto(
    string Uid,
    string ResourceName,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    DateTime UpdateTimestamp,
    IReadOnlyList<SecurityAssessmentReportDetailDto> Details
);

public sealed record ClusterRbacAssessmentReportDenormalizedDto(
    string Uid,
    string ResourceName,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,

    string Category,
    string CheckId,
    string Description,
    IReadOnlyList<string> Messages,
    string Remediation,
    int SeverityId,
    bool Success,
    string Title
);
