using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class ClusterInfraAssessmentReportDto
{
    public Guid Uid { get; init; }
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public long CriticalCount { get; init; }
    public long HighCount { get; init; }
    public long MediumCount { get; init; }

    public long LowCount { get; init; }

    //public DateTime? UpdateTimestamp { get; init; }
    public ClusterInfraAssessmentReportDetailDto[] Details { get; set; } = [];
}

public class ClusterInfraAssessmentReportDetailDto
{
    public Guid Id => Guid.NewGuid();
    public Guid MatchKey => GuidUtils.GetDeterministicGuid(SeverityId, CheckId);
    public string Category { get; init; } = string.Empty;
    public string CheckId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Messages { get; init; } = [];
    public string Remediation { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public bool Success { get; init; }
    public string Title { get; init; } = string.Empty;
}

public class ClusterInfraAssessmentReportDenormalizedDto
{
    public Guid Uid { get; init; } = Guid.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string CheckId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Messages { get; init; } = [];
    public string Remediation { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public bool Success { get; init; }
    public string Title { get; init; } = string.Empty;
}

public static class ClusterInfraAssessmentReportCrExtensions
{
    public static ClusterInfraAssessmentReportDto ToClusterInfraAssessmentReportDto(
        this OldClusterInfraAssessmentReportCr oldClusterInfraAssessmentReportCr
    )
    {
        List<ClusterInfraAssessmentReportDetailDto> clusterInfraAssessmentReportDetailDtos = [];
        foreach (Check check in oldClusterInfraAssessmentReportCr.Report?.Checks ?? [])
        {
            ClusterInfraAssessmentReportDetailDto clusterInfraAssessmentReportDetailDto = new()
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
            clusterInfraAssessmentReportDetailDtos.Add(clusterInfraAssessmentReportDetailDto);
        }

        ClusterInfraAssessmentReportDto clusterInfraAssessmentReportDto = new()
        {
            Uid =
                Guid.TryParse(oldClusterInfraAssessmentReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid
                    : new Guid(),
            ResourceName =
                oldClusterInfraAssessmentReportCr.Metadata.Labels != null &&
                oldClusterInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.name",
                    out string? resourceName
                ) ? resourceName : string.Empty,
            ResourceKind =
                oldClusterInfraAssessmentReportCr.Metadata.Labels != null &&
                oldClusterInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.kind",
                    out string? resourceKind
                ) ? resourceKind : string.Empty,
            CriticalCount = oldClusterInfraAssessmentReportCr.Report?.Summary?.CriticalCount ?? 0,
            HighCount = oldClusterInfraAssessmentReportCr.Report?.Summary?.HighCount ?? 0,
            MediumCount = oldClusterInfraAssessmentReportCr.Report?.Summary?.MediumCount ?? 0,
            LowCount = oldClusterInfraAssessmentReportCr.Report?.Summary?.LowCount ?? 0,
            Details = [.. clusterInfraAssessmentReportDetailDtos,],
        };

        return clusterInfraAssessmentReportDto;
    }

    public static IList<ClusterInfraAssessmentReportDenormalizedDto>
        ToClusterInfraAssessmentReportDetailDenormalizedDtos(
            this OldClusterInfraAssessmentReportCr oldClusterInfraAssessmentReportCr
        )
    {
        if (oldClusterInfraAssessmentReportCr is null)
        {
            throw new ArgumentNullException(nameof(oldClusterInfraAssessmentReportCr));
        }

        List<ClusterInfraAssessmentReportDenormalizedDto> clusterInfraAssessmentReportDenormalizedDtos = [];
        foreach (Check check in oldClusterInfraAssessmentReportCr.Report?.Checks ?? [])
        {
            ClusterInfraAssessmentReportDenormalizedDto clusterInfraAssessmentReportDenormalizedDto = new()
            {
                Category = check.Category,
                CheckId = check.CheckId,
                Description = check.Description,
                Messages = check.Messages,
                Remediation = check.Remediation,
                SeverityId = (int)check.Severity,
                Success = check.Success,
                Title = check.Title,
                Uid = new Guid(oldClusterInfraAssessmentReportCr?.Metadata?.Uid ?? string.Empty),
                ResourceName =
                    oldClusterInfraAssessmentReportCr?.Metadata?.Labels != null &&
                    oldClusterInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.name",
                        out string? resourceName
                    ) ? resourceName : string.Empty,
                ResourceKind =
                    oldClusterInfraAssessmentReportCr?.Metadata?.Labels != null &&
                    oldClusterInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.kind",
                        out string? resourceKind
                    ) ? resourceKind : string.Empty,
            };
            clusterInfraAssessmentReportDenormalizedDtos.Add(clusterInfraAssessmentReportDenormalizedDto);
        }

        return clusterInfraAssessmentReportDenormalizedDtos;
    }
}
