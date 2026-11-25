using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Models;

// report types by name (ScanReportType); an unknown one is refused
public sealed record ScanSubmitRequestDto(string Image, string[] ReportTypes);

public sealed record ScanSubmitResponseDto(string ScanId, bool IsExisting);

public sealed record OnDemandScanDto(
    string Id,
    string Image,
    string[] RequestedReportTypes,
    string? RequestedBy,
    string State,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    string? ImageDigest,
    string[] ProducedReportTypes,
    string? FailureMessage);

// Image: from the last scan of the digest (null once that scan record expired); the reports carry no occurrences,
// so their own image name fields are empty
public sealed record OnDemandScanResultDto(
    string Digest,
    string? Image,
    string LastScanId,
    DateTime CollectedAt,
    DateTime ExpiresAt,
    VulnerabilityReportImageDto? VulnerabilityReport,
    ExposedSecretReportImageDto? ExposedSecretReport,
    SbomReportImageDto? SbomReport);
