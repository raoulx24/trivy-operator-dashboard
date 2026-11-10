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
    int Images);

public sealed record HomeWhereIsVulnerabilityItemDto(
    string VulnerabilityId,
    string? Title,
    // the highest severity of its packages
    int SeverityId,
    bool IsKnownExploited,
    DateOnly? KevDateAdded,
    double? EpssPercentile,
    IReadOnlyList<HomeWhereIsVulnerabilityPackageDto> Packages,
    int Images,
    int Namespaces,
    int Workloads,
    IReadOnlyList<string> NamespaceNames,
    IReadOnlyList<string> ImageNames);

public sealed record HomeWhereIsVulnerabilityDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Query,
    string? Note,
    int TotalMatches,
    IReadOnlyList<HomeWhereIsVulnerabilityItemDto> Items);

// --- package: "pkg:..." = purl prefix (an "@" adds a version prefix), anything else = name substring;
// case-insensitive; grouped by package (purl without version, see PackageKey)
public sealed record HomeWhereIsPackageVersionDto(
    string Version,
    int Images,
    int Namespaces,
    int Workloads,
    // active vulnerabilities of this version of the package (all severities)
    int VulnerabilityIds,
    int? MaxSeverityId,
    IReadOnlyList<string> ImageNames);

public sealed record HomeWhereIsPackageItemDto(
    string PackageKey,
    string Name,
    string? Type,
    // from the sbom; empty when the source is the vulnerability reports
    IReadOnlyList<string> Licenses,
    IReadOnlyList<HomeWhereIsPackageVersionDto> Versions,
    int Images,
    int Namespaces);

public sealed record HomeWhereIsPackageDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Query,
    string? Note,
    // Sbom (all the packages) or VulnerabilityReports (only the vulnerable ones)
    string Source,
    int TotalMatches,
    IReadOnlyList<HomeWhereIsPackageItemDto> Items);

// --- license: SPDX id or name substring, case-insensitive; needs the sbom
public sealed record HomeWhereIsLicensePackageDto(
    string PackageKey,
    string Name,
    IReadOnlyList<string> Versions,
    int Images,
    int Namespaces);

public sealed record HomeWhereIsLicenseItemDto(
    string License,
    int TotalPackages,
    int Images,
    int Namespaces,
    IReadOnlyList<string> NamespaceNames,
    // limited, the ones in most images first
    IReadOnlyList<HomeWhereIsLicensePackageDto> Packages);

public sealed record HomeWhereIsLicenseDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Query,
    string? Note,
    int TotalMatches,
    IReadOnlyList<HomeWhereIsLicenseItemDto> Items);

// --- check: by id (KSV014, KSV-0014 and AVD-KSV-0014 are the same check) or title substring, case-insensitive
public sealed record HomeWhereIsCheckResourceDto(
    // the report type, e.g. ConfigAuditReport
    string ReportKind,
    string? NamespaceName,
    // the owner of the report (the assessed resource) when known, else the report itself
    string ResourceKind,
    string ResourceName);

public sealed record HomeWhereIsCheckItemDto(
    // ConfigAudit, RbacAssessment, InfraAssessment
    string Family,
    string CheckId,
    string Title,
    int SeverityId,
    string Remediation,
    int Resources,
    int Namespaces,
    IReadOnlyList<string> NamespaceNames,
    // limited
    IReadOnlyList<HomeWhereIsCheckResourceDto> SampleResources);

public sealed record HomeWhereIsCheckDto(
    bool IsAvailable,
    string? UnavailableReason,
    string Query,
    string? Note,
    int TotalMatches,
    IReadOnlyList<HomeWhereIsCheckItemDto> Items);
