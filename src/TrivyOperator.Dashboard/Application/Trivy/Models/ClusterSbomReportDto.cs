using TrivyOperator.Dashboard.Application.Trivy.Models.Abstracts;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterSbomReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class ClusterSbomReportDto : ISbomReportDto<ClusterSbomReportDetailDto>
{
    public Guid Uid { get; set; } = Guid.NewGuid();
    public DateTime UpdateTimestamp { get; set; } = DateTime.MinValue;
    public string ImageName { get; set; } = string.Empty;
    public string ImageTag { get; set; } = string.Empty;
    public string ImageRepository { get; set; } = string.Empty;
    public bool HasVulnerabilities { get; set; } = false;
    public int CriticalCount { get; set; } = -1;
    public int HighCount { get; set; } = -1;
    public int MediumCount { get; set; } = -1;
    public int LowCount { get; set; } = -1;
    public int UnknownCount { get; set; } = -1;
    public string RootNodeBomRef { get; set; } = string.Empty;
    public ClusterSbomReportDetailDto[] Details { get; set; } = [];
}

public class ClusterSbomReportDetailDto : ISBomReportDetailDto
{
    public Guid Id
    {
        get
        {
            if (string.IsNullOrEmpty(Purl))
            {
                if (Guid.TryParse(BomRef, out Guid bomRefGuid))
                {
                    return bomRefGuid;
                }

                return GuidUtils.GetDeterministicGuid(BomRef);
            }

            return GuidUtils.GetDeterministicGuid(Purl);
        }
    }

    public Guid MatchKey =>
        GuidUtils.GetDeterministicGuid((string.IsNullOrEmpty(Purl.Split('@')[0]) ? Name : Purl.Split('@')[0]));

    public string Name { get; set; } = string.Empty;
    public string Purl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string[][] Properties { get; set; } = [];
    public int CriticalCount { get; set; } = -1;
    public int HighCount { get; set; } = -1;
    public int MediumCount { get; set; } = -1;
    public int LowCount { get; set; } = -1;
    public int UnknownCount { get; set; } = -1;
    public string BomRef { get; set; } = string.Empty;
    public string[] DependsOn { get; set; } = [];
}

public class ClusterSbomReportDenormalizedDto
{
    public Guid Uid => Guid.NewGuid();
    public DateTime CreationTimestamp { get; set; } = DateTime.MinValue;
    public string ImageName { get; set; } = string.Empty;
    public string ImageTag { get; set; } = string.Empty;
    public string ImageRepository { get; set; } = string.Empty;
    public string RootNodeBomRef { get; set; } = string.Empty;
    public string BomRef { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Purl { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int DependenciesCount { get; set; }
    public int PropertiesCount { get; set; }
}

public static class ClusterSbomReportCrExtensions
{
    public static ClusterSbomReportDto ToClusterSbomReportDto(this OldClusterSbomReportCr oldClusterSbomReportCr)
    {
        ComponentsComponent[] allComponents = GetAllComponents(oldClusterSbomReportCr);

        IEnumerable<ClusterSbomReportDetailDto> details = allComponents.Select(component =>
            {
                ClusterSbomReportDetailDto detailDto = new()
                {
                    BomRef = component.BomRef,
                    Name = component.Name,
                    Purl = component.Purl,
                    Version = component.Version,
                    DependsOn = oldClusterSbomReportCr.Report?.Components.Dependencies
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

        ClusterSbomReportDto result = new()
        {
            Uid = Guid.TryParse(oldClusterSbomReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            UpdateTimestamp = oldClusterSbomReportCr.Report?.UpdateTimestamp ?? DateTime.MinValue,
            ImageName = oldClusterSbomReportCr.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = oldClusterSbomReportCr.Report?.Artifact?.Tag ?? string.Empty,
            ImageRepository = oldClusterSbomReportCr.Report?.Registry?.Server ?? string.Empty,
            RootNodeBomRef = oldClusterSbomReportCr.Report?.Components.Metadata.Component.BomRef ?? string.Empty,
            Details = [.. details,],
        };
        Queries.Trivy.Models.SbomReportCrExtensions.CleanupPurlsFromBomRefs(result);
        GroupDetails(result);

        return result;
    }

    public static IEnumerable<ClusterSbomReportDenormalizedDto> ToClusterSbomReportDenormalizedDtos(
        this OldClusterSbomReportCr oldClusterSbomReportCr
    )
    {
        ComponentsComponent[] allComponents = GetAllComponents(oldClusterSbomReportCr);

        IEnumerable<ClusterSbomReportDenormalizedDto> result = allComponents.Select(component =>
            {
                ClusterSbomReportDenormalizedDto detailDto = new()
                {
                    CreationTimestamp = oldClusterSbomReportCr.Metadata.CreationTimestamp ?? DateTime.MinValue,
                    ImageName = oldClusterSbomReportCr.Report?.Artifact?.Repository ?? string.Empty,
                    ImageTag = oldClusterSbomReportCr.Report?.Artifact?.Tag ?? string.Empty,
                    ImageRepository = oldClusterSbomReportCr.Report?.Registry?.Server ?? string.Empty,
                    RootNodeBomRef = oldClusterSbomReportCr.Report?.Components.Metadata.Component.BomRef ?? string.Empty,
                    BomRef = component.BomRef,
                    Name = component.Name,
                    Purl = component.Purl,
                    Version = component.Version,
                    DependenciesCount =
                        oldClusterSbomReportCr.Report?.Components.Dependencies
                            .FirstOrDefault(x => x.Ref == component.BomRef)
                            ?.DependsOn.Length ??
                        0,
                    PropertiesCount = component.Properties.Length,
                };
                return detailDto;
            }
        );

        return result;
    }

    private static void GroupDetails(ClusterSbomReportDto dto)
    {
        Dictionary<string, ClusterSbomReportDetailDto> dtoLookup = dto.Details
            .GroupBy(x => x.BomRef)
            .ToDictionary(g => g.Key, g => g.First());

        IEnumerable<ClusterSbomReportDetailDto>? filteredDtos = dto.Details.Where(dto =>
            dto.Properties.Any(p => p.Length >= 2 && p[0] == "resource:Type" && p[1] == "node") &&
            dto.Properties.Any(p => p.Length >= 1 && p[0] == "NodeRole")
        );

        foreach (ClusterSbomReportDetailDto detail in filteredDtos ?? [])
        {
            Dictionary<string, ClusterSbomReportDetailDto> allChildren = new()
            {
                {
                    detail.BomRef, detail
                },
            };
            GetDescendants(detail, dtoLookup, allChildren);
            foreach (KeyValuePair<string, ClusterSbomReportDetailDto> child in allChildren)
            {
                child.Value.Properties = [.. child.Value.Properties, ["tod.group", $"node {detail.Name}",],];
            }
        }
    }

    private static void GetDescendants(
        ClusterSbomReportDetailDto rootDto,
        Dictionary<string, ClusterSbomReportDetailDto> dtoLookup,
        Dictionary<string, ClusterSbomReportDetailDto> allChildren
    )
    {
        HashSet<string> visited = new();

        void Traverse(string bomRef)
        {
            if (visited.Contains(bomRef))
            {
                return;
            }

            visited.Add(bomRef);

            if (dtoLookup.TryGetValue(bomRef, out ClusterSbomReportDetailDto? dto))
            {
                allChildren[bomRef] = dto;
                foreach (string childRef in dto.DependsOn)
                {
                    Traverse(childRef);
                }
            }
        }

        foreach (string dep in rootDto.DependsOn)
        {
            Traverse(dep);
        }
    }


    private static ComponentsComponent[] GetAllComponents(OldClusterSbomReportCr oldClusterSbomReportCr) =>
        oldClusterSbomReportCr.Report?.Components.Metadata.Component != null ?
        [
            .. oldClusterSbomReportCr.Report?.Components.ComponentsComponents ?? [],
            oldClusterSbomReportCr.Report?.Components.Metadata.Component!,
        ] : [.. oldClusterSbomReportCr.Report?.Components.ComponentsComponents ?? [],];
}
