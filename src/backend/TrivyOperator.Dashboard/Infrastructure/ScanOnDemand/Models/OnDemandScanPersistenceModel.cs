namespace TrivyOperator.Dashboard.Infrastructure.ScanOnDemand.Models;

// flat copy of OnDemandScan; the image is stored as its normalized full name and parsed back on read
public sealed record OnDemandScanPersistenceModel(
    string Id,
    string Image,
    string[] RequestedReportTypes,
    string? RequestedBy,
    DateTime CreatedAt,
    string State,
    DateTime UpdatedAt,
    DateTime? FinishedAt,
    string? PodName,
    string? ImageDigest,
    string[] ProducedReportTypes,
    string? FailureMessage,
    DateTime? StartedAt);
