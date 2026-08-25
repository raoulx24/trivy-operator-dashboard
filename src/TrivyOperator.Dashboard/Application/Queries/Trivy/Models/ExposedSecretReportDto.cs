namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record ExposedSecretReportDto(
    string Uid,
    string NamespaceName,

    string Digest,
    string ImageNameAndTag,
    string ImageRepository,

    string ResourceName,
    string ResourceKind,
    string ResourceContainerName,

    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int UnknownCount,

    DateTime UpdateTimestamp,

    IReadOnlyList<ExposedSecretReportDetailDto> Details
);

public sealed record ExposedSecretReportImageDto(
    string Uid,
    IReadOnlyList<string> NamespaceNames,

    string Digest,
    IReadOnlyList<ExposedSecretReportImageDtoInfo> ImageInfos,

    IReadOnlyList<ExposedSecretReportResourceDto> Resources,

    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int UnknownCount,

    DateTime UpdateTimestamp,

    IReadOnlyList<ExposedSecretReportDetailDto> Details
);

public sealed record ExposedSecretReportImageDtoInfo(
    string NameAndTag,
    string Repository
);

public sealed record ExposedSecretReportResourceDto(
    string Name,
    string Kind,
    string ContainerName
);

public sealed record ExposedSecretReportDetailDto(
    string Id,
    string MatchKey,
    string Category,
    string Match,
    string RuleId,
    int SeverityId,
    string Target,
    string Title
);

public sealed record ExposedSecretReportDenormalizedDto(
    string Uid,
    string ResourceName,
    string ResourceNamespace,
    string ResourceKind,
    string ResourceContainerName,

    string ImageName,
    string ImageTag,
    string ImageDigest,
    string ImageRepository,

    DateTime UpdateTimestamp,

    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int UnknownCount,

    string Category,
    string Match,
    string RuleId,
    int SeverityId,
    string Target,
    string Title
);
