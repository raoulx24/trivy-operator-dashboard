namespace TrivyOperator.Dashboard.Application.Home.Queries.Models;

// the "where is" searches of the home page: what runs now, in the namespaces of the filter (if any)
// each DTO echoes the query; Note explains an empty or partial result (query too short, data not complete...)
// TotalMatches counts all the matches, Items are limited

// --- vulnerability: by id (CVE, GHSA, distro advisories...), prefix, case-insensitive; exact matches first
public sealed record HomeWhereIsVulnerabilityPackageDto(
    string PackageKey,
    string PackageName,
    IReadOnlyList<string> InstalledVersions,
    string? FixedVersion,
    int SeverityId,
    int Images,
    // the images that have it: indexes in the ImageNames of the item
    IReadOnlyList<int> ImageIndexes);

// the list (collapsed widget, picker): only what a row shows, the rest is in the detailed one
public sealed record HomeWhereIsVulnerabilitySeverityDto(int SeverityId, int Packages);

public sealed record HomeWhereIsVulnerabilityItemDto(
    string VulnerabilityId,
    string? Title,
    // the highest severity of its packages
    int SeverityId,
    bool IsKnownExploited,
    // the packages by severity, most severe first
    IReadOnlyList<HomeWhereIsVulnerabilitySeverityDto> PackagesBySeverity,
    int Packages,
    // the packages with a fix
    int FixablePackages,
    int Images,
    int Namespaces,
    int Workloads);

public sealed record HomeWhereIsVulnerabilityDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Query,
    string? Note,
    int TotalMatches,
    IReadOnlyList<HomeWhereIsVulnerabilityItemDto> Items);

// one vulnerability id, everything: all the images and namespaces (sorted, not capped)
public sealed record HomeWhereIsVulnerabilityDetailItemDto(
    string VulnerabilityId,
    string? Title,
    int SeverityId,
    bool IsKnownExploited,
    DateOnly? KevDateAdded,
    double? EpssPercentile,
    IReadOnlyList<HomeWhereIsVulnerabilityPackageDto> Packages,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames,
    // per image (as ImageNames), the namespaces it runs in: indexes in NamespaceNames; with the ImageIndexes of the packages,
    // that is what a page needs to filter the three lists by each other
    IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes);

// Item is null when the id (exact, case-insensitive) does not run now
public sealed record HomeWhereIsVulnerabilityDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string VulnerabilityId,
    string? Note,
    HomeWhereIsVulnerabilityDetailItemDto? Item);

// --- package: "pkg:..." = purl prefix (an "@" adds a version prefix), anything else = name substring;
// case-insensitive; one item per package version (a purl with its version; the package is the purl without version,
// see PackageKey)

// the list (collapsed widget, picker): only what a row shows, the rest is in the detailed one
public sealed record HomeWhereIsPackageItemDto(
    // the purl without version: the id of the package
    string PackageKey,
    string Version,
    // the package key with the version: pkg:deb/ubuntu/libc6@2.35-1?distro=ubuntu-22.04 (not percent-encoded)
    string Purl,
    string Name,
    string? Type,
    int Images,
    int Namespaces,
    int Workloads,
    // active vulnerabilities of this version of the package (all severities)
    int VulnerabilityIds,
    int? MaxSeverityId);

public sealed record HomeWhereIsPackageDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Query,
    string? Note,
    // Sbom (all the packages) or VulnerabilityReports (only the vulnerable ones)
    string Source,
    int TotalMatches,
    IReadOnlyList<HomeWhereIsPackageItemDto> Items);

public sealed record HomeWhereIsPackageVulnerabilityDto(
    string VulnerabilityId,
    string? Title,
    int SeverityId,
    string? FixedVersion,
    bool IsKnownExploited,
    // the images that have it: indexes in the ImageNames of the item
    IReadOnlyList<int> ImageIndexes);

// one version of a package, everything: all the images and namespaces (sorted, not capped)
public sealed record HomeWhereIsPackageDetailItemDto(
    string PackageKey,
    string Version,
    string Purl,
    string Name,
    string? Type,
    // from the sbom; empty when the source is the vulnerability reports
    IReadOnlyList<string> Licenses,
    int Images,
    int Namespaces,
    int Workloads,
    int? MaxSeverityId,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames,
    // per image (as ImageNames), the namespaces it runs in: indexes in NamespaceNames; with the ImageIndexes of the
    // vulnerabilities, that is what a page needs to filter the three lists by each other
    IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes,
    // the most severe first
    IReadOnlyList<HomeWhereIsPackageVulnerabilityDto> Vulnerabilities);

// Item is null when the package version (exact) does not run now
public sealed record HomeWhereIsPackageDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string PackageKey,
    string Version,
    string? Note,
    HomeWhereIsPackageDetailItemDto? Item);

// --- license: SPDX id or name substring, case-insensitive; needs the sbom; one item per license

// the list (collapsed widget, picker): only what a row shows, the rest is in the detailed one
public sealed record HomeWhereIsLicenseItemDto(
    string License,
    // the package versions (a purl with its version) that have it
    int Packages,
    int Images,
    int Namespaces,
    int Workloads);

public sealed record HomeWhereIsLicenseDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Query,
    string? Note,
    int TotalMatches,
    IReadOnlyList<HomeWhereIsLicenseItemDto> Items);

// one version of a package that has the license
public sealed record HomeWhereIsLicensePackageDto(
    // the purl without version: the id of the package
    string PackageKey,
    string Version,
    // the package key with the version (not percent-encoded)
    string Purl,
    string Name,
    string? Type,
    int Images,
    // the images that have it: indexes in the ImageNames of the item
    IReadOnlyList<int> ImageIndexes);

// one license (exact), everything: all the package versions, images and namespaces (sorted, not capped)
public sealed record HomeWhereIsLicenseDetailItemDto(
    string License,
    int Packages,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames,
    // per image (as ImageNames), the namespaces it runs in: indexes in NamespaceNames; with the ImageIndexes of the
    // packages, that is what a page needs to filter the three lists by each other
    IReadOnlyList<IReadOnlyList<int>> ImageNamespaceIndexes,
    // the ones in most images first
    IReadOnlyList<HomeWhereIsLicensePackageDto> PackageVersions);

// Item is null when the license (exact) does not run now
public sealed record HomeWhereIsLicenseDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string License,
    string? Note,
    HomeWhereIsLicenseDetailItemDto? Item);

// --- check: by id (KSV014, KSV-0014 and AVD-KSV-0014 are the same check) or title substring, case-insensitive
public sealed record HomeWhereIsCheckResourceDto(
    // the report type, e.g. ConfigAuditReport
    string ReportKind,
    string? NamespaceName,
    // the owner of the report (the assessed resource) when known, else the report itself
    string ResourceKind,
    string ResourceName);

// the list (collapsed widget, picker): only what a row shows, the rest is in the detailed one
public sealed record HomeWhereIsCheckItemDto(
    // ConfigAudit, RbacAssessment, InfraAssessment
    string Family,
    string CheckId,
    string Title,
    int SeverityId,
    // the assessed resources that fail it
    int Resources,
    int Namespaces);

public sealed record HomeWhereIsCheckDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Query,
    string? Note,
    int TotalMatches,
    IReadOnlyList<HomeWhereIsCheckItemDto> Items);

// one check (a family and an id, exact), everything: the denormalized rows, one per failing resource (sorted, not capped)
public sealed record HomeWhereIsCheckDetailItemDto(
    string Family,
    string CheckId,
    string Title,
    int SeverityId,
    string Category,
    string Description,
    string Remediation,
    int Resources,
    int Namespaces,
    // by namespace (cluster scoped ones last), resource kind, resource name
    IReadOnlyList<HomeWhereIsCheckResourceDto> Rows);

// Item is null when the check (exact) does not fail now
public sealed record HomeWhereIsCheckDetailDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Family,
    string CheckId,
    string? Note,
    HomeWhereIsCheckDetailItemDto? Item);
