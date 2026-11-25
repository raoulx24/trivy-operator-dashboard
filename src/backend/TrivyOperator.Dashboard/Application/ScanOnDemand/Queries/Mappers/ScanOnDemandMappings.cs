using TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Domain.ScanOnDemand.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.ScanOnDemand.Queries.Mappers;

public static class ScanOnDemandMappings
{
    public static OnDemandScanDto ToDto(this OnDemandScan scan)
        => new(
            scan.Id.Value,
            scan.Image.FullName,
            [.. scan.RequestedReportTypes.Order().Select(static x => x.ToString()),],
            scan.RequestedBy,
            scan.State.ToString(),
            scan.CreatedAt.Value,
            scan.StartedAt?.Value,
            scan.FinishedAt?.Value,
            scan.ImageDigest.IsValid ? scan.ImageDigest.Value : null,
            [.. scan.ProducedReportTypes.Order().Select(static x => x.ToString()),],
            scan.FailureMessage);

    // the sbom's per-package severities come from the result's own vulnerability report, as in SbomReportService
    public static OnDemandScanResultDto ToDto(this OnDemandScanResult result, string? image)
    {
        Dictionary<Purl, SeverityCounters> severities =
            result.VulnerabilityReport?.Vulnerabilities
                .GroupBy(static x => x.ScannedPackage.Purl)
                .ToDictionary(static g => g.Key, static g => new SeverityCounters(g.Select(static x => x.Severity)))
            ?? [];

        return new OnDemandScanResultDto(
            result.ImageDigest.Value,
            image,
            result.LastScanId.Value,
            result.CollectedAt.Value,
            result.ExpiresAt.Value,
            result.VulnerabilityReport?.ToDto(),
            result.ExposedSecretReport?.ToDto(),
            result.SbomReport?.ToImageDto(result.VulnerabilityReport?.SeverityCounters, severities));
    }
}
