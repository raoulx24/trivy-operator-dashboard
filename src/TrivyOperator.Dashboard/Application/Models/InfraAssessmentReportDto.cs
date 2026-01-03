using TrivyOperator.Dashboard.Domain.Trivy.InfraAssessmentReport;
using TrivyOperator.Dashboard.Utils;

namespace TrivyOperator.Dashboard.Application.Models;

public class InfraAssessmentReportDto
{
    public Guid Uid { get; init; } = new Guid();
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public long CriticalCount { get; init; }
    public long HighCount { get; init; }
    public long MediumCount { get; init; }
    public long LowCount { get; init; }
    //public DateTime? UpdateTimestamp { get; init; }
    public InfraAssessmentReportDetailDto[] Details { get; set; } = [];
}

public class InfraAssessmentReportDetailDto
{
    public Guid Id => Guid.NewGuid();
    public Guid MatchKey => GuidUtils.GetDeterministicGuid($"{SeverityId}{CheckId}");
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
    public static InfraAssessmentReportDto ToInfraAssessmentReportDto(this InfraAssessmentReportCr InfraAssessmentReportCr)
    {
        List<InfraAssessmentReportDetailDto> InfraAssessmentReportDetailDtos = [];
        foreach (Check check in InfraAssessmentReportCr.Report?.Checks ?? [])
        {
            InfraAssessmentReportDetailDto InfraAssessmentReportDetailDto = new()
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
            InfraAssessmentReportDetailDtos.Add(InfraAssessmentReportDetailDto);
        }

        InfraAssessmentReportDto InfraAssessmentReportDto = new()
        {
            Uid = Guid.TryParse(InfraAssessmentReportCr.Metadata.Uid, out Guid parsedGuid)
                ? parsedGuid
                : new(),
            ResourceName =
                InfraAssessmentReportCr.Metadata.Labels != null &&
                InfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.name",
                    out string? resourceName)
                    ? resourceName
                    : string.Empty,
            ResourceNamespace =
                InfraAssessmentReportCr.Metadata.Labels != null &&
                InfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.namespace",
                    out string? resourceNamespace)
                    ? resourceNamespace
                    : string.Empty,
            ResourceKind =
                InfraAssessmentReportCr.Metadata.Labels != null &&
                InfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.kind",
                    out string? resourceKind)
                    ? resourceKind
                    : string.Empty,
            CriticalCount = InfraAssessmentReportCr.Report?.Summary?.CriticalCount ?? 0,
            HighCount = InfraAssessmentReportCr.Report?.Summary?.HighCount ?? 0,
            MediumCount = InfraAssessmentReportCr.Report?.Summary?.MediumCount ?? 0,
            LowCount = InfraAssessmentReportCr.Report?.Summary?.LowCount ?? 0,
            Details = [.. InfraAssessmentReportDetailDtos],
        };

        return InfraAssessmentReportDto;
    }

    public static IList<InfraAssessmentReportDenormalizedDto> ToInfraAssessmentReportDetailDenormalizedDtos(
        this InfraAssessmentReportCr InfraAssessmentReportCr)
    {
        if (InfraAssessmentReportCr is null)
        {
            throw new ArgumentNullException(nameof(InfraAssessmentReportCr));
        }

        List<InfraAssessmentReportDenormalizedDto> InfraAssessmentReportDenormalizedDtos = [];
        foreach (Check check in InfraAssessmentReportCr.Report?.Checks ?? [])
        {
            InfraAssessmentReportDenormalizedDto InfraAssessmentReportDenormalizedDto = new()
            {
                Category = check.Category,
                CheckId = check.CheckId,
                Description = check.Description,
                Messages = check.Messages,
                Remediation = check.Remediation,
                SeverityId = (int)check.Severity,
                Success = check.Success,
                Title = check.Title,
                Uid = new Guid(InfraAssessmentReportCr?.Metadata?.Uid ?? string.Empty),
                ResourceName =
                    InfraAssessmentReportCr?.Metadata?.Labels != null &&
                    InfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.name",
                        out string? resourceName)
                        ? resourceName
                        : string.Empty,
                ResourceNamespace =
                    InfraAssessmentReportCr?.Metadata?.Labels != null &&
                    InfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.namespace",
                        out string? resourceNamespace)
                        ? resourceNamespace
                        : string.Empty,
                ResourceKind =
                    InfraAssessmentReportCr?.Metadata?.Labels != null &&
                    InfraAssessmentReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.kind",
                        out string? resourceKind)
                        ? resourceKind
                        : string.Empty,
            };
            InfraAssessmentReportDenormalizedDtos.Add(InfraAssessmentReportDenormalizedDto);
        }

        return InfraAssessmentReportDenormalizedDtos;
    }
}
