namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record RbacAssessmentReportDto(
    string Uid,
    string ResourceName,
    string ResourceNamespace,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    DateTime CreationTimestamp,
    IReadOnlyList<SecurityAssessmentReportDetailDto> Details
);

public sealed record RbacAssessmentReportDenormalizedDto(
    string Uid,
    string ResourceName,
    string ResourceNamespace,
    string Category,
    string CheckId,
    string Description,
    IReadOnlyList<string> Messages,
    string Remediation,
    int SeverityId,
    bool Success,
    string Title
);
