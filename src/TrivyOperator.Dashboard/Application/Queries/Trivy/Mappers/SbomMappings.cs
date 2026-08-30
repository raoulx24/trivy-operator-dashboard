using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;

public static class SbomReportMappings
{
    public static SbomReportDto ToDto(this SbomReport report, ReportImageOccurrence occurrence, IReadOnlyDictionary<Purl, SeverityCounters> severities)
    {
        return new SbomReportDto(
            Uid: occurrence.Metadata.Uid.Value,
            NamespaceName: occurrence.Metadata.NamespaceName.Value,
            Digest: report.ImageDigest.Value,
            ImageName: occurrence.ImageMeta.Repo.Value,
            ImageTag: occurrence.ImageMeta.Tag.Value,
            ImageRepository: occurrence.ImageMeta.Registry.Value,
            UpdateTimestamp: report.LastSeenAt.Value,
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

    public static SbomReportImageDto ToImageDto(this SbomReport report, IReadOnlyDictionary<Purl, SeverityCounters> severities)
    {
        return new SbomReportImageDto(
            Uid: GuidUtils.GetDeterministicGuid(report.ImageDigest.Value).ToString(),
            NamespaceNames:
            [
                .. report.Occurrences.Select(static x => x.Metadata.NamespaceName.Value)
                    .Distinct()
                    .OrderBy(static x => x)
            ],
            Digest: report.ImageDigest.Value,
            ImageInfos:
            [
                .. report.Occurrences.Select(static x => new SbomReportImageDtoImageInfo(
                            Name: x.ImageMeta.Repo.Value,
                            Tag: x.ImageMeta.Tag.Value,
                            Repository: x.ImageMeta.Registry.Value
                        )
                    )
                    .Distinct(),
            ],
            Resources:
            [
                .. report.Occurrences.Select(static x => new SbomReportImageResourceDto(
                        Name: x.Metadata.GetResourceName().Value,
                        Kind: x.Metadata.GetResourceKind().Value,
                        ContainerName: x.Container.Value ?? string.Empty
                    )
                )
            ],
            ComponentsCount: report.Summary.ComponentsCount,
            DependenciesCount: report.Summary.DependenciesCount,
            BomFormat: report.SbomMetadata.BomFormat,
            SpecVersion: report.SbomMetadata.SpecVersion,
            SerialNumber: report.SbomMetadata.SerialNumber.Value,
            Version: report.SbomMetadata.Version,
            UpdateTimestamp: report.LastSeenAt.Value,
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

    public static IEnumerable<SbomReportImageMinimalDto> ToMinimalDto(
        this SbomReport report,
        bool hasVulnerabilityReport)
    {
        return report.Occurrences.Select(x => new SbomReportImageMinimalDto(
            Uid: x.Metadata.Uid.Value,
            NamespaceName: x.Metadata.NamespaceName.Value,
            HasVulnerabilityReport: hasVulnerabilityReport,
            Digest: report.ImageDigest.Value,
            ImageName: x.ImageMeta.Repo.Value,
            ImageTag: x.ImageMeta.Tag.Value,
            ImageRepository: x.ImageMeta.Registry.Value
        ));
    }
}
