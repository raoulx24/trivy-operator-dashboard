namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record InfraAssessmentReportDto(
    string Uid,
    string ResourceName,
    string ResourceNamespace,
    string ResourceKind,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    IReadOnlyList<SecurityAssessmentReportDetailDto> Details
);

public sealed record InfraAssessmentReportDenormalizedDto(
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
