namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

public sealed record InfraAssessmentReportDto(
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

public sealed record InfraAssessmentReportDenormalizedDto(
    string Uid,
    string ResourceName,
    string ResourceNamespace,
    string ResourceKind,
    DateTime UpdateTimestamp,
    string Category,
    string CheckId,
    string Description,
    string Remediation,
    int SeverityId,
    string Title,
    int Count
);
