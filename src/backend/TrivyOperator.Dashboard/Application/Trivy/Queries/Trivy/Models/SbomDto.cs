namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

public sealed record SbomReportDto(
    string Uid,
    string NamespaceName,

    string Digest,
    string ImageName,
    string ImageTag,
    string ImageRegistry,

    DateTime UpdateTimestamp,

    string RootNodeBomRef,
    IReadOnlyList<SbomReportDetailDto> Details
);

public sealed record SbomReportImageDto(
    string Uid,
    IReadOnlyList<string> NamespaceNames,

    string Digest,
    IReadOnlyList<TrivyReportImageInfoDto> ImageInfos,
    string LastImageNameAndTag,
    string LastImageRegistry,

    IReadOnlyList<TrivyReportResourceInfoDto> Resources,

    int ComponentsCount,
    int DependenciesCount,

    string BomFormat,
    string SpecVersion,
    string SerialNumber,
    int Version,
    
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int UnknownCount,
    
    DateTime UpdateTimestamp,
    
    string RootNodeBomRef,
    IReadOnlyList<SbomReportDetailDto> Details
);

public sealed record SbomReportImageMinimalDto(
    string Uid,
    string ResourceNamespace,
    bool HasVulnerabilityReport,
    string Digest,
    string ImageName,
    string ImageTag,
    string ImageRegistry,
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int UnknownCount
);

public sealed record SbomReportExportDto(
    string Digest
);

public sealed record SbomExportFileDto(
    FileStream Stream,
    string FileName
);
