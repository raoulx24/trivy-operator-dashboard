using TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class ClusterRbacAssessmentReportDto
{
    public Guid Uid { get; init; } = Guid.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public long CriticalCount { get; init; }
    public long HighCount { get; init; }
    public long MediumCount { get; init; }
    public long LowCount { get; init; }
    public DateTime? UpdateTimestamp { get; init; }
    public ClusterRbacAssessmentReportDetailDto[] Details { get; init; } = [];
}

public class ClusterRbacAssessmentReportDetailDto
{
    public Guid Id => Guid.NewGuid();
    public Guid MatchKey => GuidUtils.GetDeterministicGuid($"{SeverityId}{Category}{CheckId}");
    public string Category { get; init; } = string.Empty;
    public string CheckId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Messages { get; init; } = [];
    public string Remediation { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public bool Success { get; init; }
    public string Title { get; init; } = string.Empty;
}

public class ClusterRbacAssessmentReportDenormalizedDto
{
    public Guid Uid { get; init; } = Guid.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public long CriticalCount { get; init; }
    public long HighCount { get; init; }
    public long MediumCount { get; init; }
    public long LowCount { get; init; }

    public string Category { get; init; } = string.Empty;
    public string CheckId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Messages { get; init; } = [];
    public string Remediation { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public bool Success { get; init; }
    public string Title { get; init; } = string.Empty;
}

public class ClusterRbacAssessmentReportSummaryDto
{
    public int SeverityId { get; init; } = 0;
    public int TotalCount { get; init; } = 0;
    public int DistinctCount { get; init; } = 0;
}

public static class ClusterRbacAssessmentReportCrExtensions
{
    public static ClusterRbacAssessmentReportDto ToClusterRbacAssessmentReportDto(
        this ClusterRbacAssessmentReportCr clusterRbacAssessmentReportCr
    )
    {
        List<ClusterRbacAssessmentReportDetailDto> clusterRbacAssessmentReportDetailDtos = [];
        foreach (Check check in clusterRbacAssessmentReportCr.Report?.Checks ?? [])
        {
            ClusterRbacAssessmentReportDetailDto clusterRbacAssessmentReportDetailDto = new()
            {
                Category = check.Category,
                CheckId = check.CheckId,
                Description = check.Description,
                Messages = check.Messages,
                Remediation = check.Remediation,
                SeverityId = (int)check.Severity,
                Success = check.Success,
                Title = check.Title,
            };
            clusterRbacAssessmentReportDetailDtos.Add(clusterRbacAssessmentReportDetailDto);
        }

        ClusterRbacAssessmentReportDto clusterRbacAssessmentReportDto = new()
        {
            Uid =
                Guid.TryParse(clusterRbacAssessmentReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid
                    : new Guid(),
            UpdateTimestamp = clusterRbacAssessmentReportCr.Report?.UpdateTimestamp ?? DateTime.MinValue,
            ResourceName =
                clusterRbacAssessmentReportCr.Metadata.Annotations != null &&
                clusterRbacAssessmentReportCr.Metadata.Annotations.TryGetValue(
                    "trivy-operator.resource.name",
                    out string? resourceNameFromAnnotations
                ) ? resourceNameFromAnnotations
                : clusterRbacAssessmentReportCr.Metadata.Labels != null &&
                  clusterRbacAssessmentReportCr.Metadata.Labels.TryGetValue(
                      "trivy-operator.resource.name",
                      out string? resourceNameFromLabels
                  ) ? resourceNameFromLabels : $"[{clusterRbacAssessmentReportCr.Metadata.Name}]",
            CriticalCount = clusterRbacAssessmentReportCr.Report?.Summary?.CriticalCount ?? 0,
            HighCount = clusterRbacAssessmentReportCr.Report?.Summary?.HighCount ?? 0,
            MediumCount = clusterRbacAssessmentReportCr.Report?.Summary?.MediumCount ?? 0,
            LowCount = clusterRbacAssessmentReportCr.Report?.Summary?.LowCount ?? 0,
            Details = [.. clusterRbacAssessmentReportDetailDtos,],
        };

        return clusterRbacAssessmentReportDto;
    }

    public static IEnumerable<ClusterRbacAssessmentReportDenormalizedDto> ToClusterRbacAssessmentReportDenormalizedDtos(
        this ClusterRbacAssessmentReportCr clusterRbacAssessmentReportCr
    )
    {
        IEnumerable<ClusterRbacAssessmentReportDenormalizedDto> clusterRbacAssessmentReportDetailDtos =
            (clusterRbacAssessmentReportCr.Report?.Checks ?? []).Select(check =>
                new ClusterRbacAssessmentReportDenormalizedDto
                {
                    Category = check.Category,
                    CheckId = check.CheckId,
                    Description = check.Description,
                    Messages = check.Messages,
                    Remediation = check.Remediation,
                    SeverityId = (int)check.Severity,
                    Success = check.Success,
                    Title = check.Title,
                    Uid = new Guid(clusterRbacAssessmentReportCr?.Metadata?.Uid ?? string.Empty),
                    ResourceName =
                        clusterRbacAssessmentReportCr?.Metadata.Annotations != null &&
                        clusterRbacAssessmentReportCr.Metadata.Annotations.TryGetValue(
                            "trivy-operator.resource.name",
                            out string? resourceNameFromAnnotations
                        ) ? resourceNameFromAnnotations
                        : clusterRbacAssessmentReportCr?.Metadata.Labels != null &&
                          clusterRbacAssessmentReportCr.Metadata.Labels.TryGetValue(
                              "trivy-operator.resource.name",
                              out string? resourceNameFromLabels
                          ) ? resourceNameFromLabels : $"[{clusterRbacAssessmentReportCr?.Metadata.Name}]",
                    CriticalCount = clusterRbacAssessmentReportCr?.Report?.Summary?.CriticalCount ?? 0,
                    HighCount = clusterRbacAssessmentReportCr?.Report?.Summary?.HighCount ?? 0,
                    MediumCount = clusterRbacAssessmentReportCr?.Report?.Summary?.MediumCount ?? 0,
                    LowCount = clusterRbacAssessmentReportCr?.Report?.Summary?.LowCount ?? 0,
                }
            );

        return clusterRbacAssessmentReportDetailDtos;
    }
}
