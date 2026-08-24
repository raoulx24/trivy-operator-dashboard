using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;
using TrivyOperator.Dashboard.Domain.Utils;

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
    IReadOnlyList<SbomReportImageDtoImageInfo> ImageInfos,

    IReadOnlyList<SbomReportImageResourceDto> Resources,

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

public sealed record SbomReportImageDtoImageInfo(
    string Name,
    string Tag,
    string Repository
);

public sealed record SbomReportImageResourceDto(
    string Name,
    string Kind,
    string ContainerName
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

public sealed record SbomReportExportDto(
    string Digest
);

public sealed record SbomExportFileDto(
    FileStream Stream,
    string FileName
);

public static partial class SbomReportMappings
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
            RootNodeBomRef: ToDtoBomRef(report.RootNodeBomRef),
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
                        Name: x.Resource.Name.Value,
                        Kind: x.Resource.Kind.Value,
                        ContainerName: x.Resource.Container?.Value ?? string.Empty
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
            RootNodeBomRef: ToDtoBomRef(report.RootNodeBomRef),
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

    public static SbomReportDetailDto ToDto(this Component component, SeverityCounters? severityCounters)
    {
        string id = GuidUtils.GetDeterministicGuid(
                component.Purl?.Value ?? component.Name.Value,
                component.Version.Value
            )
            .ToString();

        return new SbomReportDetailDto(
            Id: id,
            MatchKey: GuidUtils.GetDeterministicGuid(component.Purl?.Value ?? component.Name.Value).ToString(),
            Name: component.Name.Value,
            Purl: component.Purl?.Value ?? string.Empty,
            Version: component.Version.Value,
            Properties: component.Properties,
            Licenses:
            [
                .. component.Licenses.Select(static x => new SbomReportLicenseDto(Id: x.Id, Name: x.Name, Url: x.Url))
            ],
            CriticalCount: severityCounters?.CriticalCount ?? -1,
            HighCount: severityCounters?.HighCount ?? -1,
            MediumCount: severityCounters?.MediumCount ?? -1,
            LowCount: severityCounters?.LowCount ?? -1,
            UnknownCount: severityCounters?.CriticalCount ?? -1,
            BomRef: ToDtoBomRef(component.Id),
            DependsOn:
            [
                .. component.DependsOnIds.Select(ToDtoBomRef)
            ]
        );
    }

    private static string ToDtoBomRef(ComponentId value)
    {
        return Guid.TryParse(value.Value, out _) 
            ? value.Value
            : GuidUtils.GetDeterministicGuid(value.Value).ToString();
    }
}
