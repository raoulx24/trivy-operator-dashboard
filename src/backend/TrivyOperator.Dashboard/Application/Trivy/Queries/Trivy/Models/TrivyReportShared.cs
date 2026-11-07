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
    string VulnerabilityId,
    int Count
);

public sealed record SbomReportDetailDto(
    string Id,
    string MatchKey,
    string Name,
    string Purl,
    string Version,
    IReadOnlyList<SbomReportDetailPropertyDto> Properties,
    IReadOnlyList<string> Licenses,
    int? CriticalCount,
    int? HighCount,
    int? MediumCount,
    int? LowCount,
    int? UnknownCount,
    string BomRef,
    IReadOnlyList<string> DependsOn
);

public sealed record SbomReportDetailPropertyDto(
    string Name,
    string Value
);

public sealed record SecurityAssessmentReportDetailDto(
    string Id,
    string MatchKey,
    string Category,
    string CheckId,
    string Description,
    string Remediation,
    int SeverityId,
    string Title,
    int Count
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
