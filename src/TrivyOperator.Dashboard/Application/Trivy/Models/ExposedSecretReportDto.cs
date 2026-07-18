using System.ComponentModel.DataAnnotations;
using TrivyOperator.Dashboard.Domain.TrivyOld.ExposedSecretReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class ExposedSecretReportDto
{
    public Guid Uid { get; init; } = Guid.NewGuid();
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public string ResourceContainerName { get; init; } = string.Empty;
    public string ImageName { get; init; } = string.Empty;
    public string ImageTag { get; init; } = string.Empty;
    public string ImageDigest { get; init; } = string.Empty;
    public string ImageRepository { get; init; } = string.Empty;
    public int CriticalCount { get; init; }
    public int HighCount { get; init; }
    public int MediumCount { get; init; }
    public int LowCount { get; init; }
    public DateTime? UpdateTimestamp { get; init; }
    public ExposedSecretReportDetailDto[] Details { get; set; } = [];
}

public class ExposedSecretReportImageDto
{
    public Guid Uid { get; init; } = Guid.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public string ImageName { get; init; } = string.Empty;
    public string ImageTag { get; init; } = string.Empty;
    public string ImageDigest { get; init; } = string.Empty;
    public string ImageRepository { get; init; } = string.Empty;
    public List<ExposedSecretReportImageResourceDto> Resources { get; init; } = [];
    public int CriticalCount { get; init; }
    public int HighCount { get; init; }
    public int MediumCount { get; init; }
    public int LowCount { get; init; }
    public ExposedSecretReportDetailDto[] Details { get; set; } = [];
}

public class ExposedSecretReportImageResourceDto
{
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string ContainerName { get; init; } = string.Empty;
}

public class ExposedSecretReportDetailDto
{
    public Guid Id => GuidUtils.GetDeterministicGuid(SeverityId, Category, RuleId, Target);
    public Guid MatchKey => Id;
    public string Category { get; init; } = string.Empty;
    public string Match { get; init; } = string.Empty;
    public string RuleId { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public string Target { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}

public class ExposedSecretReportDenormalizedDto
{
    public Guid Uid { get; init; } = Guid.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public string ResourceContainerName { get; init; } = string.Empty;
    public string ImageName { get; init; } = string.Empty;
    public string ImageTag { get; init; } = string.Empty;
    public string ImageDigest { get; init; } = string.Empty;
    public string ImageRepository { get; init; } = string.Empty;
    public int CriticalCount { get; init; }
    public int HighCount { get; init; }
    public int MediumCount { get; init; }
    public int LowCount { get; init; }

    public string Category { get; init; } = string.Empty;
    public string Match { get; init; } = string.Empty;
    public string RuleId { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public string Target { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}

public class EsSeveritiesByNsSummaryDto
{
    public Guid Uid { get; init; }

    [Required]
    public string NamespaceName { get; init; } = string.Empty;

    [Required]
    public bool IsTotal { get; init; } = false;

    public IEnumerable<EsSeveritiesByNsSummaryDetailDto> Details { get; init; } = [];
}

public class EsSeveritiesByNsSummaryDetailDto
{
    public int Id { get; init; } = 0;
    public int TotalCount { get; init; } = 0;
    public int DistinctCount { get; init; } = 0;
}

public static class ExposedSecretReportCrExtensions
{
    public static ExposedSecretReportDto ToExposedSecretReportDto(this OldExposedSecretReportCr oldExposedSecretReportCr)
    {
        List<ExposedSecretReportDetailDto> exposedSecretReportDetailDtos = [];
        foreach (Secret secret in oldExposedSecretReportCr.Report?.Secrets ?? [])
        {
            ExposedSecretReportDetailDto exposedSecretReportDetailDto = new()
            {
                Category = secret.Category,
                Match = secret.Match,
                RuleId = secret.RuleId,
                SeverityId = (int)secret.Severity,
                Target = secret.Target,
                Title = secret.Title,
            };
            exposedSecretReportDetailDtos.Add(exposedSecretReportDetailDto);
        }

        ExposedSecretReportDto exposedSecretReportDto = new()
        {
            Uid = Guid.TryParse(oldExposedSecretReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            UpdateTimestamp = oldExposedSecretReportCr.Report?.UpdateTimestamp ?? DateTime.MinValue,
            ResourceName =
                oldExposedSecretReportCr.Metadata.Labels != null &&
                oldExposedSecretReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.name",
                    out string? resourceName
                ) ? resourceName : string.Empty,
            ResourceNamespace =
                oldExposedSecretReportCr.Metadata.Labels != null &&
                oldExposedSecretReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.namespace",
                    out string? resourceNamespace
                ) ? resourceNamespace : string.Empty,
            ResourceKind =
                oldExposedSecretReportCr.Metadata.Labels != null &&
                oldExposedSecretReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.kind",
                    out string? resourceKind
                ) ? resourceKind : string.Empty,
            ResourceContainerName =
                oldExposedSecretReportCr.Metadata.Labels != null &&
                oldExposedSecretReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.container.name",
                    out string? resourceContainerName
                ) ? resourceContainerName : string.Empty,
            ImageName = oldExposedSecretReportCr.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = oldExposedSecretReportCr.Report?.Artifact?.Tag ?? string.Empty,
            ImageDigest = oldExposedSecretReportCr.Report?.Artifact?.Digest ?? string.Empty,
            ImageRepository = oldExposedSecretReportCr.Report?.Registry?.Server ?? string.Empty,
            CriticalCount = oldExposedSecretReportCr.Report?.Summary?.CriticalCount ?? 0,
            HighCount = oldExposedSecretReportCr.Report?.Summary?.HighCount ?? 0,
            MediumCount = oldExposedSecretReportCr.Report?.Summary?.MediumCount ?? 0,
            LowCount = oldExposedSecretReportCr.Report?.Summary?.LowCount ?? 0,
            Details = [.. exposedSecretReportDetailDtos,],
        };

        return exposedSecretReportDto;
    }

    public static ExposedSecretReportImageDto ToExposedSecretReportImageDto(
        this IGrouping<ImageGroupKey, OldExposedSecretReportCr> groupedExposedSecretReportCr,
        IEnumerable<int>? excludedSeverities = null
    )
    {
        excludedSeverities ??= [];
        int[] excludedSeveritiesArray = [.. excludedSeverities,];
        List<ExposedSecretReportImageResourceDto> eseirDtos = [];
        foreach (OldExposedSecretReportCr vr in groupedExposedSecretReportCr)
        {
            ExposedSecretReportImageResourceDto eseirDto = new()
            {
                Name =
                    vr.Metadata.Labels.TryGetValue("trivy-operator.resource.name", out string? name) ? name
                        : string.Empty,
                ContainerName =
                    vr.Metadata.Labels.TryGetValue("trivy-operator.container.name", out string? containerName)
                        ? containerName : string.Empty,
                Kind = vr.Metadata.Labels.TryGetValue("trivy-operator.resource.kind", out string? kind) ? kind
                    : string.Empty,
            };
            eseirDtos.Add(eseirDto);
        }

        OldExposedSecretReportCr? latestExposedSecretReportCr = groupedExposedSecretReportCr
            .OrderByDescending(x => x.Report?.UpdateTimestamp)
            .FirstOrDefault();
        List<ExposedSecretReportDetailDto> exposedSecretReportDetailDtos = [];
        foreach (Secret? secret in latestExposedSecretReportCr?.Report?.Secrets ?? [])
        {
            if (!excludedSeveritiesArray.Contains((int)secret.Severity))
            {
                ExposedSecretReportDetailDto exposedSecretReportDetailDto = new()
                {
                    Category = secret.Category,
                    Match = secret.Match,
                    RuleId = secret.RuleId,
                    SeverityId = (int)secret.Severity,
                    Target = secret.Target,
                    Title = secret.Title,
                };
                exposedSecretReportDetailDtos.Add(exposedSecretReportDetailDto);
            }
        }

        ExposedSecretReportImageDto exposedSecretReportImageDto = new()
        {
            Uid = new Guid(latestExposedSecretReportCr?.Metadata.Uid ?? string.Empty),
            ResourceNamespace =
                latestExposedSecretReportCr?.Metadata.Labels != null &&
                latestExposedSecretReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.namespace",
                    out string? resourceNamespace
                ) ? resourceNamespace : string.Empty,
            Resources = eseirDtos,
            ImageName = latestExposedSecretReportCr?.Report?.Artifact?.Repository ?? string.Empty,
            ImageTag = latestExposedSecretReportCr?.Report?.Artifact?.Tag ?? string.Empty,
            ImageDigest = latestExposedSecretReportCr?.Report?.Artifact?.Digest ?? string.Empty,
            ImageRepository = latestExposedSecretReportCr?.Report?.Registry?.Server ?? string.Empty,
            CriticalCount = latestExposedSecretReportCr?.Report?.Summary?.CriticalCount ?? 0,
            HighCount = latestExposedSecretReportCr?.Report?.Summary?.HighCount ?? 0,
            MediumCount = latestExposedSecretReportCr?.Report?.Summary?.MediumCount ?? 0,
            LowCount = latestExposedSecretReportCr?.Report?.Summary?.LowCount ?? 0,
            Details = [.. exposedSecretReportDetailDtos,],
        };

        return exposedSecretReportImageDto;
    }

    public static IEnumerable<ExposedSecretReportDenormalizedDto> ToExposedSecretReportDenormalizedDtos(
        this OldExposedSecretReportCr oldExposedSecretReportCr
    )
    {
        IEnumerable<ExposedSecretReportDenormalizedDto> exposedSecretReportDenormalizedDtos =
            (oldExposedSecretReportCr.Report?.Secrets ?? []).Select(secret => new ExposedSecretReportDenormalizedDto
                {
                    Category = secret.Category,
                    Match = secret.Match,
                    RuleId = secret.RuleId,
                    SeverityId = (int)secret.Severity,
                    Target = secret.Target,
                    Title = secret.Title,
                    Uid = new Guid(oldExposedSecretReportCr?.Metadata?.Uid ?? string.Empty),
                    ResourceName =
                        oldExposedSecretReportCr?.Metadata?.Labels != null &&
                        oldExposedSecretReportCr.Metadata.Labels.TryGetValue(
                            "trivy-operator.resource.name",
                            out string? resourceName
                        ) ? resourceName : string.Empty,
                    ResourceNamespace =
                        oldExposedSecretReportCr?.Metadata?.Labels != null &&
                        oldExposedSecretReportCr.Metadata.Labels.TryGetValue(
                            "trivy-operator.resource.namespace",
                            out string? resourceNamespace
                        ) ? resourceNamespace : string.Empty,
                    ResourceKind =
                        oldExposedSecretReportCr?.Metadata?.Labels != null &&
                        oldExposedSecretReportCr.Metadata.Labels.TryGetValue(
                            "trivy-operator.resource.kind",
                            out string? resourceKind
                        ) ? resourceKind : string.Empty,
                    ResourceContainerName =
                        oldExposedSecretReportCr?.Metadata?.Labels != null &&
                        oldExposedSecretReportCr.Metadata.Labels.TryGetValue(
                            "trivy-operator.container.name",
                            out string? resourceContainerName
                        ) ? resourceContainerName : string.Empty,
                    ImageName = oldExposedSecretReportCr?.Report?.Artifact?.Repository ?? string.Empty,
                    ImageTag = oldExposedSecretReportCr?.Report?.Artifact?.Tag ?? string.Empty,
                    ImageDigest = oldExposedSecretReportCr?.Report?.Artifact?.Digest ?? string.Empty,
                    ImageRepository = oldExposedSecretReportCr?.Report?.Registry?.Server ?? string.Empty,
                    CriticalCount = oldExposedSecretReportCr?.Report?.Summary?.CriticalCount ?? 0,
                    HighCount = oldExposedSecretReportCr?.Report?.Summary?.HighCount ?? 0,
                    MediumCount = oldExposedSecretReportCr?.Report?.Summary?.MediumCount ?? 0,
                    LowCount = oldExposedSecretReportCr?.Report?.Summary?.LowCount ?? 0,
                }
            );

        return exposedSecretReportDenormalizedDtos;
    }
}
