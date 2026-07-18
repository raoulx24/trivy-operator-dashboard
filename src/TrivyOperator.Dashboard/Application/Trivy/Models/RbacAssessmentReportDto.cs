using TrivyOperator.Dashboard.Domain.TrivyOld.RbacAssessmentReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class RbacAssessmentReportDto
{
    public Guid Uid { get; init; } = Guid.NewGuid();
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public int CriticalCount { get; init; }
    public int HighCount { get; init; }
    public int MediumCount { get; init; }
    public int LowCount { get; init; }
    public DateTime? CreationTimestamp { get; init; }
    public RbacAssessmentReportDetailDto[] Details { get; set; } = [];
}

public class RbacAssessmentReportDetailDto
{
    public Guid Id => Guid.NewGuid();
    public Guid MatchKey => GuidUtils.GetDeterministicGuid(SeverityId, Category, CheckId);
    public string Category { get; init; } = string.Empty;
    public string CheckId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Messages { get; init; } = [];
    public string Remediation { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public bool Success { get; init; }
    public string Title { get; init; } = string.Empty;
}

public class RbacAssessmentReportDenormalizedDto
{
    public Guid Uid { get; init; } = Guid.NewGuid();
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;
    public string CheckId { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string[] Messages { get; init; } = [];
    public string Remediation { get; init; } = string.Empty;
    public int SeverityId { get; init; }
    public bool Success { get; init; }
    public string Title { get; init; } = string.Empty;
}

public static class RbacAssessmentReportCrExtensions
{
    public static RbacAssessmentReportDto ToRbacAssessmentReportDto(this OldRbacAssessmentReportCr oldRbacAssessmentReportCr)
    {
        List<RbacAssessmentReportDetailDto> rbacAssessmentReportDetailDtos = [];
        foreach (Check check in oldRbacAssessmentReportCr.Report?.Checks ?? [])
        {
            RbacAssessmentReportDetailDto rbacAssessmentReportDetailDto = new()
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
            rbacAssessmentReportDetailDtos.Add(rbacAssessmentReportDetailDto);
        }

        RbacAssessmentReportDto rbacAssessmentReportDto = new()
        {
            Uid = Guid.TryParse(oldRbacAssessmentReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            CreationTimestamp = oldRbacAssessmentReportCr.Metadata.CreationTimestamp ?? DateTime.MinValue,
            ResourceName =
                oldRbacAssessmentReportCr.Metadata.Annotations != null &&
                oldRbacAssessmentReportCr.Metadata.Annotations.TryGetValue(
                    "trivy-operator.resource.name",
                    out string? resourceNameFromAnnotations
                ) ? resourceNameFromAnnotations
                : oldRbacAssessmentReportCr.Metadata.Labels != null &&
                  oldRbacAssessmentReportCr.Metadata.Labels.TryGetValue(
                      "trivy-operator.resource.name",
                      out string? resourceNameFromLabels
                  ) ? resourceNameFromLabels : $"[{oldRbacAssessmentReportCr.Metadata.Name}]",
            ResourceNamespace = oldRbacAssessmentReportCr.Metadata.NamespaceProperty,
            CriticalCount = oldRbacAssessmentReportCr.Report?.Summary?.CriticalCount ?? 0,
            HighCount = oldRbacAssessmentReportCr.Report?.Summary?.HighCount ?? 0,
            MediumCount = oldRbacAssessmentReportCr.Report?.Summary?.MediumCount ?? 0,
            LowCount = oldRbacAssessmentReportCr.Report?.Summary?.LowCount ?? 0,
            Details = [.. rbacAssessmentReportDetailDtos,],
        };

        return rbacAssessmentReportDto;
    }

    public static IList<RbacAssessmentReportDenormalizedDto> ToRbacAssessmentReportDenormalizedDtos(
        this OldRbacAssessmentReportCr oldRbacAssessmentReportCr
    )
    {
        List<RbacAssessmentReportDenormalizedDto> rbacAssessmentReportDetailDtos = [];
        string resourceName =
            oldRbacAssessmentReportCr.Metadata.Annotations != null &&
            oldRbacAssessmentReportCr.Metadata.Annotations.TryGetValue(
                "trivy-operator.resource.name",
                out string? resourceNameFromAnnotations
            ) ? resourceNameFromAnnotations
            : oldRbacAssessmentReportCr.Metadata.Labels != null &&
              oldRbacAssessmentReportCr.Metadata.Labels.TryGetValue(
                  "trivy-operator.resource.name",
                  out string? resourceNameFromLabels
              ) ? resourceNameFromLabels : $"[{oldRbacAssessmentReportCr.Metadata.Name}]";
        string resourceNamespace = oldRbacAssessmentReportCr?.Metadata.NamespaceProperty ?? string.Empty;

        foreach (Check check in oldRbacAssessmentReportCr?.Report?.Checks ?? [])
        {
            RbacAssessmentReportDenormalizedDto rbacAssessmentReportDenormalizedDto = new()
            {
                Category = check.Category,
                CheckId = check.CheckId,
                Description = check.Description,
                Messages = check.Messages,
                Remediation = check.Remediation,
                SeverityId = (int)check.Severity,
                Success = check.Success,
                Title = check.Title,
                ResourceName = resourceName,
                ResourceNamespace = resourceNamespace,
            };
            rbacAssessmentReportDetailDtos.Add(rbacAssessmentReportDenormalizedDto);
        }

        return rbacAssessmentReportDetailDtos;
    }
}
