using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class ClusterSbomReportMappings
{
    public static ClusterSbomReportDto ToDto(
        this ClusterSbomReport report,
        IReadOnlyDictionary<Purl, SeverityCounters> severities)
    {
        ReportImageOccurrence occurrence = report.Occurrence;

        return new ClusterSbomReportDto(
            Uid: occurrence.Metadata.Uid.Value,
            UpdateTimestamp: report.LastSeenAt.Value,

            ImageName: occurrence.ImageMeta.Repo.Value,
            ImageTag: occurrence.ImageMeta.Tag.Value,
            ImageRepository: occurrence.ImageMeta.Registry.Value,

            RootNodeBomRef: report.RootNodeBomRef.ToDtoBomRef(),

            Details:
            [
                .. report.Components.Select(x =>
                {
                    SeverityCounters? counters =
                        x.Purl is { } purl && severities.TryGetValue(purl, out SeverityCounters value)
                            ? value
                            : null;

                    return x.ToDto(counters);
                }),
            ]
        );
    }

    public static IEnumerable<ClusterSbomReportDenormalizedDto> ToDenormalizedDtos(
        this ClusterSbomReport report)
    {
        ReportImageOccurrence occurrence = report.Occurrence;

        return report.Components.Select(component =>
            new ClusterSbomReportDenormalizedDto(
                CreationTimestamp: report.LastSeenAt.Value,

                ImageName: occurrence.ImageMeta.Repo.Value,
                ImageTag: occurrence.ImageMeta.Tag.Value,
                ImageRepository: occurrence.ImageMeta.Registry.Value,

                RootNodeBomRef: report.RootNodeBomRef.ToDtoBomRef(),

                BomRef: component.Id.ToDtoBomRef(),
                Name: component.Name.Value,
                Purl: component.Purl?.Value ?? string.Empty,
                Version: component.Version.Value,

                DependenciesCount: component.DependsOnIds.Length,
                PropertiesCount: component.Properties.Count
            )
        );
    }
    
    public static SbomReportImageMinimalDto ToMinimalDto(
        this ClusterSbomReport report,
        bool hasVulnerabilityReport)
    {
        return new SbomReportImageMinimalDto(
            Uid: report.Occurrence.Metadata.Uid.Value,
            NamespaceName: report.Occurrence.Metadata.NamespaceName.Value,
            HasVulnerabilityReport: hasVulnerabilityReport,
            Digest: new Digest().Value,
            ImageName: report.Occurrence.ImageMeta.Repo.Value,
            ImageTag: report.Occurrence.ImageMeta.Tag.Value,
            ImageRepository: report.Occurrence.ImageMeta.Registry.Value
        );
    }
}
