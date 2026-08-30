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
    DateTime UpdateTimestampZ,
    IReadOnlyList<SecurityAssessmentReportDetailDto> Details
);

public sealed record ConfigAuditReportDenormalizedDto(
    string Uid,
    string ResourceName,
    string ResourceNamespace,
    string ResourceKind,
    string Category,
    string CheckId,
    string Description,
    IReadOnlyList<string> Messages,
    string Remediation,
    int SeverityId,
    bool Success,
    string Title
);
