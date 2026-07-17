using TrivyOperator.Dashboard.Application.Trivy.Models.Abstracts;
using TrivyOperator.Dashboard.Domain.TrivyOld.SbomReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class SbomReportDto : ISbomReportDto<SbomReportDetailDto>
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public DateTime CreationTimestamp { get; set; } = DateTime.MinValue;
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public string ResourceContainerName { get; init; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public string ImageTag { get; set; } = string.Empty;
    public string ImageDigest { get; set; } = string.Empty;
    public string ImageRepository { get; set; } = string.Empty;
    public DateTime? UpdateTimestamp { get; init; }
    public string RootNodeBomRef { get; set; } = string.Empty;
    public SbomReportDetailDto[] Details { get; set; } = [];
}

public class SbomReportImageDto : ISbomReportDto<SbomReportDetailDto>
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public DateTime CreationTimestamp { get; set; } = DateTime.MinValue;
    public string ResourceNamespace { get; init; } = string.Empty;
    public string ImageName { get; set; } = string.Empty;
    public string ImageTag { get; set; } = string.Empty;
    public string ImageDigest { get; set; } = string.Empty;
    public string ImageRepository { get; set; } = string.Empty;
    public SbomReportImageResourceDto[] Resources { get; set; } = [];
    public bool HasVulnerabilities { get; set; } = false;
    public long CriticalCount { get; set; } = -1;
    public long HighCount { get; set; } = -1;
    public long MediumCount { get; set; } = -1;
    public long LowCount { get; set; } = -1;
    public long UnknownCount { get; set; } = -1;
    public string RootNodeBomRef { get; set; } = string.Empty;
    public SbomReportDetailDto[] Details { get; set; } = [];
}

public class SbomReportImageResourceDto
{
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string ContainerName { get; init; } = string.Empty;
}

public class SbomReportImageMinimalDto
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public string ImageName { get; set; } = string.Empty;
    public string ImageTag { get; set; } = string.Empty;
    public string ImageDigest { get; set; } = string.Empty;
    public string ImageRepository { get; set; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public bool HasVulnerabilities { get; set; } = false;
    public long CriticalCount { get; set; } = -1;
    public long HighCount { get; set; } = -1;
    public long MediumCount { get; set; } = -1;
    public long LowCount { get; set; } = -1;
    public long UnknownCount { get; set; } = -1;
}

public class SbomReportDetailDto : ISBomReportDetailDto
{
    public Guid Id => GuidUtils.GetDeterministicGuid(Purl, Properties);

    public Guid MatchKey {
        get
        {
            if (string.IsNullOrWhiteSpace(Purl) && string.IsNullOrWhiteSpace(Version))
            {
                return GuidUtils.GetDeterministicGuid(Name.Split('/')[^1]);
            }
            
            return GuidUtils.GetDeterministicGuid(
                (string.IsNullOrEmpty(Purl.Split('@')[0]) ? Name : Purl.Split('@')[0])
            );
        }
    }
    public string Name { get; set; } = string.Empty;
    public string Purl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string[][] Properties { get; set; } = [];
    public string[] Licenses { get; set; } = [];
    public long CriticalCount { get; set; } = -1;
    public long HighCount { get; set; } = -1;
    public long MediumCount { get; set; } = -1;
    public long LowCount { get; set; } = -1;
    public long UnknownCount { get; set; } = -1;
    public string BomRef { get; set; } = string.Empty;
    public string[] DependsOn { get; set; } = [];
}

public class SbomReportExportDto
{
    public string NamespaceName { get; set; } = string.Empty;
    public string Digest { get; set; } = string.Empty;
}

public static partial class SbomReportCrExtensions
{
    public static SbomReportDto ToSbomReportDto(this OldSbomReportCr oldSbomReportCr)
    {
        ComponentsComponent[] allComponents = oldSbomReportCr.Report?.Components.Metadata.Component != null ?
        [
            .. oldSbomReportCr.Report?.Components.ComponentsComponents ?? [],
            oldSbomReportCr.Report?.Components.Metadata.Component!,
        ] : [.. oldSbomReportCr.Report?.Components.ComponentsComponents ?? [],];

        IEnumerable<SbomReportDetailDto> details = allComponents.Select(component =>
            {
                SbomReportDetailDto detailDto = new()
                {
                    BomRef = component.BomRef,
                    Name = component.Name,
                    Purl = component.Purl,
                    Version = component.Version,
                    DependsOn = oldSbomReportCr.Report?.Components.Dependencies
                                    .FirstOrDefault(x => x.Ref == component.BomRef)
                                    ?.DependsOn ??
                                [],
                    Properties =
                    [
                        .. component.Properties.Select(x => new[]
                            {
                                x.Name.Replace("aquasecurity:trivy:", string.Empty), x.Value,
                            }
                        ),
                    ],
                    Licenses =
                    [
                        .. component.Licenses?.Select(x => x.License?.Name ?? string.Empty)
                               .Where(x => !string.IsNullOrWhiteSpace(x)) ??
                           [],
                    ],
                };

                return detailDto;
            }
        );

        SbomReportDto result = new()
        {
            Uid = Guid.TryParse(oldSbomReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            CreationTimestamp = oldSbomReportCr.Metadata.CreationTimestamp ?? DateTime.MinValue,
            UpdateTimestamp = oldSbomReportCr.Report?.UpdateTimestamp ?? DateTime.MinValue,
            ResourceName =
                oldSbomReportCr.Metadata.Labels != null &&
                oldSbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.name", out string? resourceName)
                    ? resourceName : string.Empty,
            ResourceNamespace =
                oldSbomReportCr.Metadata.Labels != null &&
                oldSbomReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.namespace",
                    out string? resourceNamespace
                ) ? resourceNamespace : string.Empty,
            ResourceKind =
                oldSbomReportCr.Metadata.Labels != null &&
                oldSbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.kind", out string? resourceKind)
                    ? resourceKind : string.Empty,
            ResourceContainerName =
                oldSbomReportCr.Metadata.Labels != null &&
                oldSbomReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.container.name",
                    out string? resourceContainerName
                ) ? resourceContainerName : string.Empty,
            ImageName = oldSbomReportCr.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = oldSbomReportCr.Report?.Artifact?.Tag ?? string.Empty,
            ImageDigest = oldSbomReportCr.Report?.Artifact?.Digest ?? string.Empty,
            ImageRepository = oldSbomReportCr.Report?.Registry?.Server ?? string.Empty,
            RootNodeBomRef = oldSbomReportCr.Report?.Components.Metadata.Component.BomRef ?? string.Empty,
            Details = [.. details,],
        };
        CleanupPurlsFromBomRefs(result);

        return result;
    }

    public static SbomReportImageDto ToSbomReportImageDto(
        this IGrouping<ImageGroupKey, OldSbomReportCr> groupedSbomReportCr
    )
    {
        //SbomReportCr[] sbomReportCrs = [.. groupedSbomReportCr];
        OldSbomReportCr firstOldSbomReportCr = groupedSbomReportCr.First();
        ComponentsComponent[] allComponents = firstOldSbomReportCr.Report?.Components.Metadata.Component != null ?
        [
            .. firstOldSbomReportCr.Report?.Components.ComponentsComponents ?? [],
            firstOldSbomReportCr.Report?.Components.Metadata.Component!,
        ] : [.. firstOldSbomReportCr.Report?.Components.ComponentsComponents ?? [],];
        IEnumerable<SbomReportDetailDto> details = allComponents.Select(component =>
            {
                SbomReportDetailDto detailDto = new()
                {
                    BomRef = component.BomRef,
                    Name = component.Name,
                    Purl = component.Purl,
                    Version = component.Version,
                    DependsOn = firstOldSbomReportCr.Report?.Components.Dependencies
                                    .FirstOrDefault(x => x.Ref == component.BomRef)
                                    ?.DependsOn ??
                                [],
                    Properties =
                    [
                        .. component.Properties.Select(x => new[]
                            {
                                x.Name.Replace("aquasecurity:trivy:", string.Empty), x.Value,
                            }
                        ),
                    ],
                };
                return detailDto;
            }
        );
        SbomReportImageDto result = new()
        {
            Uid = Guid.TryParse(firstOldSbomReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            CreationTimestamp = firstOldSbomReportCr.Metadata.CreationTimestamp ?? DateTime.MinValue,
            ResourceNamespace = firstOldSbomReportCr.Metadata.NamespaceProperty,
            ImageName = firstOldSbomReportCr.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = firstOldSbomReportCr.Report?.Artifact?.Tag ?? string.Empty,
            ImageDigest = firstOldSbomReportCr.Report?.Artifact?.Digest ?? string.Empty,
            ImageRepository = firstOldSbomReportCr.Report?.Registry?.Server ?? string.Empty,
            Resources =
            [
                .. groupedSbomReportCr.Select(sbomReportCr => new SbomReportImageResourceDto
                    {
                        Name =
                            sbomReportCr.Metadata.Labels != null &&
                            sbomReportCr.Metadata.Labels.TryGetValue(
                                "trivy-operator.resource.name",
                                out string? resourceName
                            ) ? resourceName : string.Empty,
                        Kind =
                            sbomReportCr.Metadata.Labels != null &&
                            sbomReportCr.Metadata.Labels.TryGetValue(
                                "trivy-operator.resource.kind",
                                out string? resourceKind
                            ) ? resourceKind : string.Empty,
                        ContainerName =
                            sbomReportCr.Metadata.Labels != null &&
                            sbomReportCr.Metadata.Labels.TryGetValue(
                                "trivy-operator.container.name",
                                out string? containerName
                            ) ? containerName : string.Empty,
                    }
                ),
            ],
            RootNodeBomRef = firstOldSbomReportCr.Report?.Components.Metadata.Component.BomRef ?? string.Empty,
            Details = [.. details,],
        };
        CleanupPurlsFromBomRefs(result);
        return result;
    }

    public static SbomReportImageMinimalDto ToSbomReportImageMinimalDto(
        this IGrouping<ImageGroupKey, OldSbomReportCr> groupedSbomReportCr
    )
    {
        //SbomReportCr[] sbomReportCrs = [.. groupedSbomReportCr];
        OldSbomReportCr firstOldSbomReportCr = groupedSbomReportCr.First();
        return new SbomReportImageMinimalDto
        {
            Uid = Guid.TryParse(firstOldSbomReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            ImageName = firstOldSbomReportCr.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = firstOldSbomReportCr.Report?.Artifact?.Tag ?? string.Empty,
            ImageDigest = firstOldSbomReportCr.Report?.Artifact?.Digest ?? string.Empty,
            ImageRepository = firstOldSbomReportCr.Report?.Registry?.Server ?? string.Empty,
            ResourceNamespace = firstOldSbomReportCr.Metadata.NamespaceProperty,
        };
    }

    public static SbomReportImageResourceDto ToSbomReportImageResourceDto(this OldSbomReportCr oldSbomReportCr) => new()
    {
        Name =
            oldSbomReportCr.Metadata.Labels != null &&
            oldSbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.name", out string? resourceName)
                ? resourceName : string.Empty,
        Kind = oldSbomReportCr.Metadata.Labels != null &&
               oldSbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.kind", out string? resourceKind)
            ? resourceKind : string.Empty,
        ContainerName = oldSbomReportCr.Metadata.Labels != null &&
                        oldSbomReportCr.Metadata.Labels.TryGetValue(
                            "trivy-operator.container.name",
                            out string? containerName
                        ) ? containerName : string.Empty,
    };

    public static void CleanupPurlsFromBomRefs<TSBomReportDetailDto>(ISbomReportDto<TSBomReportDetailDto> sbomReportDto)
        where TSBomReportDetailDto : ISBomReportDetailDto
    {
        Dictionary<string, string> nonGuidToGuidMap = sbomReportDto.Details.Where(d => !Guid.TryParse(d.BomRef, out _))
            .GroupBy(d => d.BomRef)
            .ToDictionary(g => g.Key, g => GuidUtils.GetDeterministicGuid(g.Key).ToString());

        if (nonGuidToGuidMap.Count == 0)
        {
            return; // No non-GUID BomRefs to convert
        }

        foreach (TSBomReportDetailDto detail in sbomReportDto.Details)
        {
            if (nonGuidToGuidMap.TryGetValue(detail.BomRef, out string? valueFromBomRef))
            {
                detail.BomRef = valueFromBomRef;
            }

            for (int i = 0; i < detail.DependsOn.Length; i++)
            {
                if (nonGuidToGuidMap.TryGetValue(detail.DependsOn[i], out string? valueFromDependsOn))
                {
                    detail.DependsOn[i] = valueFromDependsOn;
                }
            }
        }

        if (nonGuidToGuidMap.TryGetValue(sbomReportDto.RootNodeBomRef, out string? valueFromRootNodeBomRef))
        {
            sbomReportDto.RootNodeBomRef = valueFromRootNodeBomRef;
        }
    }
}
