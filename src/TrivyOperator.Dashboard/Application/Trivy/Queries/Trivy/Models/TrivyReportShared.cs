namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

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
    IReadOnlyList<string> Licenses,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int UnknownCount,
    string BomRef,
    IReadOnlyList<string> DependsOn
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

public sealed record TrivyReportImageInfoDto(
    string NameAndTag,
    string Registry
);

public sealed record TrivyReportResourceInfoDto(
    string Name,
    string NamespaceName,
    string Kind,
    string ContainerName
);
