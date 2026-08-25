namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record VulnerabilityReportDetailDto(
    // public Guid Id => GuidUtils.GetDeterministicGuid(VulnerabilityId, Resource, InstalledVersion, Target);
    // public Guid MatchKey => Id;
    string Id,
    string MatchKey,
    string FixedVersion,
    string InstalledVersion,
    DateTime? LastModifiedDate,
    string PackageUrl,
    string? PrimaryLink,
    DateTime? PublishedDate,
    string Resource,
    decimal Score,
    int SeverityId,
    string Target,
    string Title,
    string VulnerabilityId
);

public sealed record SbomReportDetailDto(
    string Id,
    string MatchKey,
    string Name,
    string Purl,
    string Version,
    IReadOnlyDictionary<string, string> Properties,
    IReadOnlyList<SbomReportLicenseDto> Licenses,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int UnknownCount,
    string BomRef,
    IReadOnlyList<string> DependsOn
);

public sealed record SbomReportLicenseDto(
    string? Id,
    string? Name,
    Uri? Url
);

public sealed record SbomReportImageMinimalDto(
    string Uid,
    string NamespaceName,
    bool HasVulnerabilityReport,
    string Digest,
    string ImageName,
    string ImageTag,
    string ImageRepository
);

public sealed record SecurityAssessmentReportDetailDto(
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
