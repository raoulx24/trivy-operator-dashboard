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
    string? Title,
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

// one package of a new vulnerability: the row (vulnerability id, package) first seen since Since
public sealed record HomeNewVulnerabilityPackageDto(
    string PackageKey,
    string PackageName,
    IReadOnlyList<string> InstalledVersions,
    string? FixedVersion,
    int SeverityId,
    DateTime? Published,
    DateTime FirstSeenAt,
    // the current fix exists since (a lower bound of the waiting time); null without a fix
    DateTime? FixSince,
    // the fix appeared after the row was first seen; null when the row came with its fix
    DateTime? FixAppearedAt,
    int Images,
    // the images that have it: indexes in the ImageNames of the item
    IReadOnlyList<int> ImageIndexes);

// one vulnerability id of the new ones, everything: its packages first seen since Since (only these), and the images and
// namespaces they run in (sorted, not capped)
public sealed record HomeNewVulnerabilityDetailItemDto(
    string VulnerabilityId,
    string? Title,
    // the highest severity of its new packages
    int SeverityId,
    bool IsKnownExploited,
    DateOnly? KevDateAdded,
    double? EpssPercentile,
    IReadOnlyList<HomeNewVulnerabilityPackageDto> Packages,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames,
    // per image (as ImageNames), the namespaces it runs in: indexes in NamespaceNames; with the ImageIndexes of the packages,
    // that is what a page needs to filter the three lists by each other
    IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes);

// Item is null when the id has no row first seen since Since (in the severities and namespaces asked)
public sealed record HomeNewVulnerabilityDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string VulnerabilityId,
    DateTime Since,
    string? Note,
    HomeNewVulnerabilityDetailItemDto? Item);

// active vulnerabilities whose fix appeared since Since (FixAppearedAt >= Since; the ones that came with their fix
// are not here)
public sealed record HomeNewFixesDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime Since,
    DateTime? FixTrackingStartedAt,
    HomeTotalsDto Totals,
    IReadOnlyList<HomeActiveVulnerabilityDto> Items);

// one package of a vulnerability with a new fix: the row (vulnerability id, package) whose fix appeared since Since
public sealed record HomeNewFixPackageDto(
    string PackageKey,
    string PackageName,
    IReadOnlyList<string> InstalledVersions,
    string FixedVersion,
    int SeverityId,
    DateTime? Published,
    // null when the row existed before the fixed version tracking started (its age is unknown)
    DateTime? FirstSeenAt,
    // the fix appeared at (>= Since)
    DateTime FixAppearedAt,
    int Images,
    // the images that have it: indexes in the ImageNames of the item
    IReadOnlyList<int> ImageIndexes);

// one vulnerability id with new fixes, everything: its packages whose fix appeared since Since (only these), and the
// images and namespaces they run in (sorted, not capped)
public sealed record HomeNewFixDetailItemDto(
    string VulnerabilityId,
    string? Title,
    // the highest severity of its packages with a new fix
    int SeverityId,
    bool IsKnownExploited,
    DateOnly? KevDateAdded,
    double? EpssPercentile,
    IReadOnlyList<HomeNewFixPackageDto> Packages,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames,
    // per image (as ImageNames), the namespaces it runs in: indexes in NamespaceNames
    IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes);

// Item is null when the id has no row with a fix that appeared since Since (in the severities and namespaces asked)
public sealed record HomeNewFixDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string VulnerabilityId,
    DateTime Since,
    string? Note,
    HomeNewFixDetailItemDto? Item);

// fixable active vulnerabilities, oldest fix first
public sealed record HomeFixWaitingDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime? FixTrackingStartedAt,
    HomeTotalsDto Totals,
    IReadOnlyList<HomeActiveVulnerabilityDto> Items);

// one package of a vulnerability waiting for its fix to be applied: the row (vulnerability id, package) with a fix
public sealed record HomeFixWaitingPackageDto(
    string PackageKey,
    string PackageName,
    IReadOnlyList<string> InstalledVersions,
    string FixedVersion,
    int SeverityId,
    DateTime? Published,
    // null when the row existed before the fixed version tracking started (its age is unknown)
    DateTime? FirstSeenAt,
    // the current fix exists since (a lower bound of the waiting time); null without history
    DateTime? FixSince,
    // the fix appeared after the row was first seen; null when the row came with its fix
    DateTime? FixAppearedAt,
    int Images,
    // the images that have it: indexes in the ImageNames of the item
    IReadOnlyList<int> ImageIndexes);

// one vulnerability id waiting for fixes, everything: all its packages with a fix, and the images and namespaces they run
// in (sorted, not capped)
public sealed record HomeFixWaitingDetailItemDto(
    string VulnerabilityId,
    string? Title,
    // the highest severity of its packages with a fix
    int SeverityId,
    bool IsKnownExploited,
    DateOnly? KevDateAdded,
    double? EpssPercentile,
    IReadOnlyList<HomeFixWaitingPackageDto> Packages,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames,
    // per image (as ImageNames), the namespaces it runs in: indexes in NamespaceNames
    IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes);

// Item is null when the id has no row with a fix running now (in the severities and namespaces asked)
public sealed record HomeFixWaitingDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string VulnerabilityId,
    string? Note,
    HomeFixWaitingDetailItemDto? Item);

// known exploited (CISA KEV or ENISA EUVD) vulnerabilities running now, one item per vulnerability id
// the list (widget): only what a row shows, the rest is in the detailed one
public sealed record HomeKnownExploitedItemDto(
    string VulnerabilityId,
    string? Title,
    // the highest severity of its packages
    int SeverityId,
    DateOnly? KevDateAdded,
    bool? KevKnownRansomwareCampaignUse,
    DateTime? EuvdExploitedSince,
    double? EpssPercentile,
    // known exploited since Since (KEV addition or EUVD exploitation moment)
    bool IsNewSince,
    int Packages,
    // the packages with a fix
    int FixablePackages,
    int Images,
    int Namespaces,
    int Workloads);

public sealed record HomeKnownExploitedDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime Since,
    int NewSinceCount,
    HomeTotalsDto Totals,
    IReadOnlyList<HomeKnownExploitedItemDto> Items);

// one package of a known exploited vulnerability
public sealed record HomeKnownExploitedPackageDto(
    string PackageKey,
    string PackageName,
    IReadOnlyList<string> InstalledVersions,
    string? FixedVersion,
    int SeverityId,
    int Images,
    // the images that have it: indexes in the ImageNames of the item
    IReadOnlyList<int> ImageIndexes);

// one known exploited vulnerability id, everything: the KEV and EUVD data, its packages, and all the images and namespaces
// (sorted, not capped)
public sealed record HomeKnownExploitedDetailItemDto(
    string VulnerabilityId,
    string? Title,
    int SeverityId,
    // the earliest of the KEV addition and the EUVD exploitation moment
    DateTime? ExploitedSince,
    bool IsNewSince,
    DateOnly? KevDateAdded,
    DateOnly? KevDueDate,
    bool? KevKnownRansomwareCampaignUse,
    string? KevRequiredAction,
    string? EuvdId,
    DateTime? EuvdExploitedSince,
    double? EpssPercentile,
    IReadOnlyList<HomeKnownExploitedPackageDto> Packages,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames,
    // per image (as ImageNames), the namespaces it runs in: indexes in NamespaceNames; with the ImageIndexes of the packages,
    // that is what a page needs to filter the three lists by each other
    IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes);

// Item is null when the id (exact, case-insensitive) is not known exploited and running now (in the namespaces asked)
public sealed record HomeKnownExploitedDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string VulnerabilityId,
    DateTime Since,
    string? Note,
    HomeKnownExploitedDetailItemDto? Item);

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

// one row per running image and namespace, denormalized (OS family, OS cycle and its end of life date next to the image)
// OsVersion is the version as Trivy reports it (e.g. 3.19.1); OsEolFrom comes from its end of life cycle (3.19)
// NamespaceName is null for a cluster scoped image; OsEolFrom is null when unknown; not sorted
public sealed record HomeBaseOsImageRowDto(
    string OsName,
    string OsVersion,
    DateOnly? OsEolFrom,
    DateOnly? OsEoasFrom,
    string? NamespaceName,
    string ImageRegistry,
    string ImageName,
    string ImageTag,
    // empty for a cluster scoped image
    string ImageDigest);

public sealed record HomeBaseOsImagesDto(
    bool IsAvailable,
    string? UnavailableReason,
    IReadOnlyList<HomeBaseOsImageRowDto> Items);

// failing checks grouped by check id; Resources = distinct reports (one per resource)
public sealed record HomeFailingCheckDto(
    // ConfigAudit, RbacAssessment, InfraAssessment
    string Family,
    string CheckId,
    string Title,
    string Category,
    int SeverityId,
    int Resources,
    int Namespaces,
    bool HasClusterScopedResources);

public sealed record HomeFailingChecksDto(
    bool IsAvailable,
    string? UnavailableReason,
    int TotalChecks,
    IReadOnlyList<HomeFailingCheckDto> Items);

// one failing check (a family and an id, exact), everything: the description, the remediation and one row per failing
// resource (sorted, not capped); the rows are the ones of "where is... check"
public sealed record HomeFailingCheckDetailItemDto(
    string Family,
    string CheckId,
    string Title,
    int SeverityId,
    string Category,
    string Description,
    string Remediation,
    int Resources,
    int Namespaces,
    bool HasClusterScopedResources,
    // by namespace (cluster scoped ones last), resource kind, resource name
    IReadOnlyList<HomeWhereIsCheckResourceDto> Rows);

// Item is null when the check (exact) does not fail now (in the namespaces asked)
public sealed record HomeFailingCheckDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Family,
    string CheckId,
    string? Note,
    HomeFailingCheckDetailItemDto? Item);

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
    // the base OS of the image, as the report says; null when it has none
    string? OsFamily,
    string? OsName,
    // null when unknown
    bool? IsOsEndOfLife,
    // the next three count distinct vulnerability ids of the requested severities, over the whole image (not only the
    // requested namespaces)
    int VulnerabilityIds,
    // with a fix for at least one package of the image
    int FixableVulnerabilityIds,
    int KnownExploitedVulnerabilityIds,
    // the highest severity among those ids; null when the image has none
    int? HighestSeverityId);

public sealed record HomeOldestImagesDto(
    bool IsAvailable,
    string? UnavailableReason,
    DateTime? HistoryStartedAt,
    IReadOnlyList<HomeOldestImageDto> Items);

// secrets of one type (the title of the rule that matched, e.g. "AWS Access Key ID") with the severity of the rule
public sealed record HomeExposedSecretTypeCountDto(string Type, int SeverityId, int Count);

// one running image (one digest) with exposed secrets; the secret values and files are never sent, only counts
// ByType: most severe first, then most secrets, then by name
public sealed record HomeExposedSecretImageDto(
    string Digest,
    string Registry,
    string Repository,
    string Tag,
    int SecretCount,
    // the highest severity of its secrets
    int SeverityId,
    int Workloads,
    int Namespaces,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<HomeExposedSecretTypeCountDto> ByType);

// running images with exposed secrets, the worst first
// ImagesScanned / LastScanAt: what the exposed secret reports cover (to tell "no secrets" from "no data"), in the
// namespaces asked; TotalSecrets / TotalImages are not capped by the limit (Items is)
public sealed record HomeExposedSecretsDto(
    bool IsAvailable,
    string? UnavailableReason,
    int ImagesScanned,
    DateTime? LastScanAt,
    int TotalSecrets,
    int TotalImages,
    IReadOnlyList<HomeExposedSecretImageDto> Items);

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
