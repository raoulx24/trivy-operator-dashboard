using TrivyOperator.Dashboard.Domain.TrivyOld.InfraAssessmentReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class InfraAssessmentReportDto
{
    public Guid Uid { get; init; }
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public int CriticalCount { get; init; }
    public int HighCount { get; init; }
    public int MediumCount { get; init; }

    public int LowCount { get; init; }

    //public DateTime? UpdateTimestamp { get; init; }
    public InfraAssessmentReportDetailDto[] Details { get; set; } = [];
}

public class InfraAssessmentReportDetailDto
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

public class InfraAssessmentReportDenormalizedDto
{
    public Guid Uid { get; init; } = Guid.Empty;
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
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

public static class InfraAssessmentReportCrExtensions
{
    public static InfraAssessmentReportDto ToInfraAssessmentReportDto(
        this OldInfraAssessmentReportCr oldInfraAssessmentReportCr
    )
    {
        List<InfraAssessmentReportDetailDto> infraAssessmentReportDetailDtos = [];
        foreach (Check check in oldInfraAssessmentReportCr.Report?.Checks ?? [])
        {
            InfraAssessmentReportDetailDto infraAssessmentReportDetailDto = new()
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
            infraAssessmentReportDetailDtos.Add(infraAssessmentReportDetailDto);
        }

        InfraAssessmentReportDto infraAssessmentReportDto = new()
        {
            Uid = Guid.TryParse(oldInfraAssessmentReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            ResourceName =
                oldInfraAssessmentReportCr.Metadata.Labels != null &&
                oldInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.name",
                    out string? resourceName
                ) ? resourceName : string.Empty,
            ResourceNamespace =
                oldInfraAssessmentReportCr.Metadata.Labels != null &&
                oldInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.namespace",
                    out string? resourceNamespace
                ) ? resourceNamespace : string.Empty,
            ResourceKind =
                oldInfraAssessmentReportCr.Metadata.Labels != null &&
                oldInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.kind",
                    out string? resourceKind
                ) ? resourceKind : string.Empty,
            CriticalCount = oldInfraAssessmentReportCr.Report?.Summary?.CriticalCount ?? 0,
            HighCount = oldInfraAssessmentReportCr.Report?.Summary?.HighCount ?? 0,
            MediumCount = oldInfraAssessmentReportCr.Report?.Summary?.MediumCount ?? 0,
            LowCount = oldInfraAssessmentReportCr.Report?.Summary?.LowCount ?? 0,
            Details = [.. infraAssessmentReportDetailDtos,],
        };

        return infraAssessmentReportDto;
    }

    public static IList<InfraAssessmentReportDenormalizedDto> ToInfraAssessmentReportDetailDenormalizedDtos(
        this OldInfraAssessmentReportCr oldInfraAssessmentReportCr
    )
    {
        if (oldInfraAssessmentReportCr is null)
        {
            throw new ArgumentNullException(nameof(oldInfraAssessmentReportCr));
        }

        List<InfraAssessmentReportDenormalizedDto> infraAssessmentReportDenormalizedDtos = [];
        foreach (Check check in oldInfraAssessmentReportCr.Report?.Checks ?? [])
        {
            InfraAssessmentReportDenormalizedDto infraAssessmentReportDenormalizedDto = new()
            {
                Category = check.Category,
                CheckId = check.CheckId,
                Description = check.Description,
                Messages = check.Messages,
                Remediation = check.Remediation,
                SeverityId = (int)check.Severity,
                Success = check.Success,
                Title = check.Title,
                Uid = new Guid(oldInfraAssessmentReportCr?.Metadata?.Uid ?? string.Empty),
                ResourceName =
                    oldInfraAssessmentReportCr?.Metadata?.Labels != null &&
                    oldInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.name",
                        out string? resourceName
                    ) ? resourceName : string.Empty,
                ResourceNamespace =
                    oldInfraAssessmentReportCr?.Metadata?.Labels != null &&
                    oldInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.namespace",
                        out string? resourceNamespace
                    ) ? resourceNamespace : string.Empty,
                ResourceKind =
                    oldInfraAssessmentReportCr?.Metadata?.Labels != null &&
                    oldInfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.kind",
                        out string? resourceKind
                    ) ? resourceKind : string.Empty,
            };
            infraAssessmentReportDenormalizedDtos.Add(infraAssessmentReportDenormalizedDto);
        }

        return infraAssessmentReportDenormalizedDtos;
    }
}
