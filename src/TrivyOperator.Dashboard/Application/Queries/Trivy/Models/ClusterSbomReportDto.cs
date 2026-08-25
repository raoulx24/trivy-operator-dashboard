namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record ClusterSbomReportDto(
    string Uid,
    DateTime UpdateTimestamp,

    string ImageName,
    string ImageTag,
    string ImageRepository,

    string RootNodeBomRef,
    IReadOnlyList<SbomReportDetailDto> Details
);

public sealed record ClusterSbomReportDenormalizedDto(
    DateTime CreationTimestamp,

    string ImageName,
    string ImageTag,
    string ImageRepository,

    string RootNodeBomRef,

    string BomRef,
    string Name,
    string Purl,
    string Version,

    int DependenciesCount,
    int PropertiesCount
);
