namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record ConfigAuditReportDto(
    string Uid,
    string ResourceName,
    string ResourceNamespace,
    string ResourceKind,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    DateTime UpdateTimestamp,
    IReadOnlyList<SecurityAssessmentReportDetailDto> Details
);

public sealed record ConfigAuditReportDenormalizedDto(
    string Uid,
    string ResourceName,
    string ResourceNamespace,
    string ResourceKind,
    DateTime UpdateTimestamp,
    string Category,
    string CheckId,
    string Description,
    IReadOnlyList<string> Messages,
    string Remediation,
    int SeverityId,
    bool Success,
    string Title
);
