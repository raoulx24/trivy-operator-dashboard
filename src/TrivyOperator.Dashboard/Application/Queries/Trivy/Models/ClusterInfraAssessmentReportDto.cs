namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record ClusterInfraAssessmentReportDto(
    string Uid,
    string ResourceName,
    string ResourceKind,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    IReadOnlyList<ClusterInfraAssessmentReportDetailDto> Details
);

public sealed record ClusterInfraAssessmentReportDetailDto(
    string Id,
    string MatchKey,
    string Category,
    string CheckId,
    string Description,
    IReadOnlyList<string> Messages,
    string Remediation,
    int SeverityId,
    bool Success,
    string Title
);

public sealed record ClusterInfraAssessmentReportDenormalizedDto(
    string Uid,
    string ResourceName,
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
