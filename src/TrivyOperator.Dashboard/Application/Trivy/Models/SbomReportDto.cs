using TrivyOperator.Dashboard.Application.Trivy.Models.Abstracts;
using TrivyOperator.Dashboard.Domain.Trivy.SbomReport;
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
    public Guid Id => GuidUtils.GetDeterministicGuid(Purl);

    public Guid MatchKey =>
        GuidUtils.GetDeterministicGuid($"{(string.IsNullOrEmpty(Purl.Split('@')[0]) ? Name : Purl.Split('@')[0])}");

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
    public static SbomReportDto ToSbomReportDto(this SbomReportCr sbomReportCr)
    {
        ComponentsComponent[] allComponents = sbomReportCr.Report?.Components.Metadata.Component != null
            ?
            [
                .. sbomReportCr.Report?.Components.ComponentsComponents ?? [],
                sbomReportCr.Report?.Components.Metadata.Component!,
            ] : [.. sbomReportCr.Report?.Components.ComponentsComponents ?? [],];

        IEnumerable<SbomReportDetailDto> details = allComponents.Select(component =>
            {
                SbomReportDetailDto detailDto = new()
                {
                    BomRef = component.BomRef,
                    Name = component.Name,
                    Purl = component.Purl,
                    Version = component.Version,
                    DependsOn = sbomReportCr.Report?.Components.Dependencies
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
            Uid = Guid.TryParse(sbomReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            CreationTimestamp = sbomReportCr.Metadata.CreationTimestamp ?? DateTime.MinValue,
            UpdateTimestamp = sbomReportCr.Report?.UpdateTimestamp ?? DateTime.MinValue,
            ResourceName =
                sbomReportCr.Metadata.Labels != null &&
                sbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.name", out string? resourceName)
                    ? resourceName : string.Empty,
            ResourceNamespace =
                sbomReportCr.Metadata.Labels != null &&
                sbomReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.namespace",
                    out string? resourceNamespace
                ) ? resourceNamespace : string.Empty,
            ResourceKind =
                sbomReportCr.Metadata.Labels != null &&
                sbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.kind", out string? resourceKind)
                    ? resourceKind : string.Empty,
            ResourceContainerName =
                sbomReportCr.Metadata.Labels != null &&
                sbomReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.container.name",
                    out string? resourceContainerName
                ) ? resourceContainerName : string.Empty,
            ImageName = sbomReportCr.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = sbomReportCr.Report?.Artifact?.Tag ?? string.Empty,
            ImageDigest = sbomReportCr.Report?.Artifact?.Digest ?? string.Empty,
            ImageRepository = sbomReportCr.Report?.Registry?.Server ?? string.Empty,
            RootNodeBomRef = sbomReportCr.Report?.Components.Metadata.Component.BomRef ?? string.Empty,
            Details = [.. details,],
        };
        CleanupPurlsFromBomRefs(result);

        return result;
    }

    public static SbomReportImageDto ToSbomReportImageDto(
        this IGrouping<ImageGroupKey, SbomReportCr> groupedSbomReportCr
    )
    {
        //SbomReportCr[] sbomReportCrs = [.. groupedSbomReportCr];
        SbomReportCr firstSbomReportCr = groupedSbomReportCr.First();
        ComponentsComponent[] allComponents = firstSbomReportCr.Report?.Components.Metadata.Component != null
            ?
            [
                .. firstSbomReportCr.Report?.Components.ComponentsComponents ?? [],
                firstSbomReportCr.Report?.Components.Metadata.Component!,
            ] : [.. firstSbomReportCr.Report?.Components.ComponentsComponents ?? [],];
        IEnumerable<SbomReportDetailDto> details = allComponents.Select(component =>
            {
                SbomReportDetailDto detailDto = new()
                {
                    BomRef = component.BomRef,
                    Name = component.Name,
                    Purl = component.Purl,
                    Version = component.Version,
                    DependsOn = firstSbomReportCr.Report?.Components.Dependencies
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
            Uid = Guid.TryParse(firstSbomReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            CreationTimestamp = firstSbomReportCr.Metadata.CreationTimestamp ?? DateTime.MinValue,
            ResourceNamespace = firstSbomReportCr.Metadata.NamespaceProperty,
            ImageName = firstSbomReportCr.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = firstSbomReportCr.Report?.Artifact?.Tag ?? string.Empty,
            ImageDigest = firstSbomReportCr.Report?.Artifact?.Digest ?? string.Empty,
            ImageRepository = firstSbomReportCr.Report?.Registry?.Server ?? string.Empty,
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
            RootNodeBomRef = firstSbomReportCr.Report?.Components.Metadata.Component.BomRef ?? string.Empty,
            Details = [.. details,],
        };
        CleanupPurlsFromBomRefs(result);
        return result;
    }

    public static SbomReportImageMinimalDto ToSbomReportImageMinimalDto(
        this IGrouping<ImageGroupKey, SbomReportCr> groupedSbomReportCr
    )
    {
        //SbomReportCr[] sbomReportCrs = [.. groupedSbomReportCr];
        SbomReportCr firstSbomReportCr = groupedSbomReportCr.First();
        return new SbomReportImageMinimalDto
        {
            Uid = Guid.TryParse(firstSbomReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            ImageName = firstSbomReportCr.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = firstSbomReportCr.Report?.Artifact?.Tag ?? string.Empty,
            ImageDigest = firstSbomReportCr.Report?.Artifact?.Digest ?? string.Empty,
            ImageRepository = firstSbomReportCr.Report?.Registry?.Server ?? string.Empty,
            ResourceNamespace = firstSbomReportCr.Metadata.NamespaceProperty,
        };
    }

    public static SbomReportImageResourceDto ToSbomReportImageResourceDto(this SbomReportCr sbomReportCr) => new()
    {
        Name =
            sbomReportCr.Metadata.Labels != null &&
            sbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.name", out string? resourceName)
                ? resourceName : string.Empty,
        Kind = sbomReportCr.Metadata.Labels != null &&
               sbomReportCr.Metadata.Labels.TryGetValue("trivy-operator.resource.kind", out string? resourceKind)
            ? resourceKind : string.Empty,
        ContainerName = sbomReportCr.Metadata.Labels != null &&
                        sbomReportCr.Metadata.Labels.TryGetValue(
                            "trivy-operator.container.name",
                            out string? containerName
                        ) ? containerName : string.Empty,
    };

    public static void CleanupPurlsFromBomRefs<TSBomReportDetailDto>(ISbomReportDto<TSBomReportDetailDto> sbomReportDto)
        where TSBomReportDetailDto : ISBomReportDetailDto
    {
        Dictionary<string, string> nonGuidToGuidMap = sbomReportDto.Details.Where(d => !Guid.TryParse(d.BomRef, out _))
            .ToDictionary(d => d.BomRef, d => GuidUtils.GetDeterministicGuid(d.BomRef).ToString());

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
