namespace TrivyOperator.Dashboard.Application.Home.Queries.Models;

// the DTOs of the home page, one per widget; each one tells whether its data sources are available
// (UnavailableReason is set when IsAvailable is false)
// units, everywhere: VulnerabilityIds = distinct vulnerability ids (risk), ActiveVulnerabilities = distinct
// (vulnerability id, package) rows (fix work), Images / Namespaces / Workloads = reach
// severities are severity ids (0 critical ... 4 unknown); dates are UTC

public sealed record HomeTotalsDto(
    int VulnerabilityIds,
    int ActiveVulnerabilities,
    int Images,
    int Namespaces,
    int Workloads);

public sealed record HomeActiveVulnerabilityDto(
    string VulnerabilityId,
    string PackageKey,
    string PackageName,
    int SeverityId,
    string? FixedVersion,
    DateTime? Published,
    // null when the row existed before the fixed version tracking started (its age is unknown)
    DateTime? FirstSeenAt,
    // the current fix exists since (a lower bound of the waiting time)
    DateTime? FixSince,
    // the fix appeared after the row was first seen; null when the row came with its fix
    DateTime? FixAppearedAt,
    bool IsKnownExploited,
    double? EpssPercentile,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames);

// active vulnerabilities first seen since Since
public sealed record HomeNewVulnerabilitiesDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime Since,
    DateTime? FixTrackingStartedAt,
    HomeTotalsDto Totals,
    IReadOnlyList<HomeActiveVulnerabilityDto> Items);

// active vulnerabilities whose fix appeared since Since (FixAppearedAt >= Since; the ones that came with their fix
// are not here)
public sealed record HomeNewFixesDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime Since,
    DateTime? FixTrackingStartedAt,
    HomeTotalsDto Totals,
    IReadOnlyList<HomeActiveVulnerabilityDto> Items);

// fixable active vulnerabilities, oldest fix first
public sealed record HomeFixWaitingDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime? FixTrackingStartedAt,
    HomeTotalsDto Totals,
    IReadOnlyList<HomeActiveVulnerabilityDto> Items);

// known exploited (CISA KEV or ENISA EUVD) vulnerabilities running now, one item per vulnerability id
public sealed record HomeKnownExploitedItemDto(
    string VulnerabilityId,
    int SeverityId,
    IReadOnlyList<string> PackageNames,
    DateOnly? KevDateAdded,
    bool? KevKnownRansomwareCampaignUse,
    DateTime? EuvdExploitedSince,
    double? EpssPercentile,
    // known exploited since Since (KEV addition or EUVD exploitation moment)
    bool IsNewSince,
    bool HasFix,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames);

public sealed record HomeKnownExploitedDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime Since,
    int NewSinceCount,
    HomeTotalsDto Totals,
    IReadOnlyList<HomeKnownExploitedItemDto> Items);

// one image (one digest) running on an OS cycle; the digest is empty for a cluster scoped image
public sealed record HomeBaseOsImageDto(
    string ImageNameAndTag,
    string ImageDigest,
    string ImageRegistry);

// one OS cycle (family + cycle, e.g. alpine 3.19 for the 3.19.0 and 3.19.1 images) with its end of life status
// Version is the cycle; for an OS without end of life data it is the version as Trivy reports it
// ImageDetails and NamespaceNames: sorted and capped by the limit query param (the counts are not)
public sealed record HomeBaseOsItemDto(
    string Family,
    string Version,
    // Pending, Ok, NotApplicable, Error (see EolStatus)
    string Status,
    string? NotApplicableReason,
    string? ProductLabel,
    string? Cycle,
    bool? IsEol,
    DateOnly? EolFrom,
    bool? IsEoas,
    DateOnly? EoasFrom,
    string? LatestVersion,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<HomeBaseOsImageDto> ImageDetails,
    IReadOnlyList<string> NamespaceNames);

public sealed record HomeBaseOsDto(
    bool IsAvailable,
    string? UnavailableReason,
    IReadOnlyList<HomeBaseOsItemDto> Items);

// failing checks grouped by check id; Resources = distinct reports (one per resource)
public sealed record HomeFailingCheckDto(
    // ConfigAudit, RbacAssessment, InfraAssessment
    string Family,
    string CheckId,
    string Title,
    int SeverityId,
    int Resources,
    int Namespaces,
    bool HasClusterScopedResources);

public sealed record HomeFailingChecksDto(
    bool IsAvailable,
    string? UnavailableReason,
    int TotalChecks,
    IReadOnlyList<HomeFailingCheckDto> Items);

// running images by the moment their digest was first seen by the history (based on history)
public sealed record HomeOldestImageDto(
    string Digest,
    string Registry,
    string Repository,
    string Tag,
    DateTime FirstSeenAt,
    int Workloads,
    int Namespaces,
    IReadOnlyList<string> NamespaceNames,
    // distinct vulnerability ids of the requested severities
    int VulnerabilityIds);

public sealed record HomeOldestImagesDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime? HistoryStartedAt,
    IReadOnlyList<HomeOldestImageDto> Items);

// active vulnerabilities grouped by package (PackageKey, i.e. purl without version)
public sealed record HomePackageHotspotDto(
    string PackageKey,
    string PackageName,
    int VulnerabilityIds,
    int FixableVulnerabilityIds,
    int Images,
    int Namespaces);

public sealed record HomePackageHotspotsDto(
    bool IsAvailable,
    string? UnavailableReason,
    IReadOnlyList<HomePackageHotspotDto> Items);

// per vulnerability id with an EPSS score: reach against the EPSS percentile
public sealed record HomeReachEpssItemDto(
    string VulnerabilityId,
    int SeverityId,
    double EpssPercentile,
    bool IsKnownExploited,
    int Images,
    int Namespaces);

public sealed record HomeReachEpssDto(
    bool IsAvailable,
    string? UnavailableReason,
    IReadOnlyList<HomeReachEpssItemDto> Items);

// data health
public sealed record HomeWatcherIssueDto(
    string KubernetesObjectType,
    string? NamespaceName,
    string Status,
    string MitigationMessage);

public sealed record HomeReportFreshnessDto(
    // the report kind, e.g. VulnerabilityReport
    string Kind,
    int Count,
    DateTime? OldestScanAt,
    DateTime? NewestScanAt);

public sealed record HomeSourceStatusDto(
    // KEV, EPSS, EUVD, EOL
    string Source,
    bool IsEnabled,
    // the newest fetch seen in the entries (feed level status is not kept)
    DateTime? LastFetchedAt,
    int ErrorCount);

public sealed record HomeDataHealthDto(
    int WatchersTotal,
    int WatchersOk,
    IReadOnlyList<HomeWatcherIssueDto> WatcherIssues,
    IReadOnlyList<HomeReportFreshnessDto> Reports,
    IReadOnlyList<HomeSourceStatusDto> Sources,
    bool IsVrHistoryEnabled,
    DateTime? HistoryStartedAt,
    DateTime? FixTrackingStartedAt,
    DateTime GeneratedAt);
