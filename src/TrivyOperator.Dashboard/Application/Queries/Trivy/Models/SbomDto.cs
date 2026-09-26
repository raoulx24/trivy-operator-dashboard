namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record SbomReportDto(
    string Uid,
    string NamespaceName,

    string Digest,
    string ImageName,
    string ImageTag,
    string ImageRepository,

    DateTime UpdateTimestamp,

    string RootNodeBomRef,
    IReadOnlyList<SbomReportDetailDto> Details
);

public sealed record SbomReportImageDto(
    string Uid,
    IReadOnlyList<string> NamespaceNames,

    string Digest,
    IReadOnlyList<TrivyReportImageInfoDto> ImageInfos,

    IReadOnlyList<TrivyReportResourceInfoDto> Resources,

    int ComponentsCount,
    int DependenciesCount,

    string BomFormat,
    string SpecVersion,
    string SerialNumber,
    int Version,
    DateTime UpdateTimestamp,
    
    string RootNodeBomRef,
    IReadOnlyList<SbomReportDetailDto> Details
);

public sealed record SbomReportExportDto(
    string Digest
);

public sealed record SbomExportFileDto(
    FileStream Stream,
    string FileName
);
