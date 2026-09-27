namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

public sealed record ClusterSbomReportDto(
    string Uid,
    DateTime UpdateTimestamp,

    string ImageName,
    string ImageTag,
    string ImageRepository,
    
    int CriticalCount,
    int HighCount,
    int MediumCount,
    int LowCount,
    int UnknownCount,

    string RootNodeBomRef,
    IReadOnlyList<SbomReportDetailDto> Details
);

public sealed record ClusterSbomReportDenormalizedDto(
    DateTime UpdateTimestamp,

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
