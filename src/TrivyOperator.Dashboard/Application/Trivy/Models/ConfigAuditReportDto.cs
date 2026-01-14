using TrivyOperator.Dashboard.Domain.Trivy.ConfigAuditReport;
using TrivyOperator.Dashboard.Domain.Utils;

namespace TrivyOperator.Dashboard.Application.Trivy.Models;

public class ConfigAuditReportDto
{
    public Guid Uid { get; init; }
    public string ResourceName { get; init; } = string.Empty;
    public string ResourceNamespace { get; init; } = string.Empty;
    public string ResourceKind { get; init; } = string.Empty;
    public long CriticalCount { get; init; }
    public long HighCount { get; init; }
    public long MediumCount { get; init; }
    public long LowCount { get; init; }
    public DateTime? UpdateTimestamp { get; init; }
    public ConfigAuditReportDetailDto[] Details { get; set; } = [];
}

public class ConfigAuditReportDetailDto
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

public class ConfigAuditReportDenormalizedDto
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

public class ConfigAuditReportSummaryDto
{
    public string NamespaceName { get; init; } = string.Empty;
    public int SeverityId { get; init; } = 0;
    public string Kind { get; init; } = string.Empty;
    public int TotalCount { get; init; } = 0;
    public int DistinctCount { get; init; } = 0;
}

public static class ConfigAuditReportCrExtensions
{
    public static ConfigAuditReportDto ToConfigAuditReportDto(this ConfigAuditReportCr configAuditReportCr)
    {
        List<ConfigAuditReportDetailDto> configAuditReportDetailDtos = [];
        foreach (Check check in configAuditReportCr.Report?.Checks ?? [])
        {
            ConfigAuditReportDetailDto configAuditReportDetailDto = new()
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
            configAuditReportDetailDtos.Add(configAuditReportDetailDto);
        }

        ConfigAuditReportDto configAuditReportDto = new()
        {
            Uid = Guid.TryParse(configAuditReportCr.Metadata.Uid, out Guid parsedGuid) ? parsedGuid : new Guid(),
            UpdateTimestamp = configAuditReportCr.Report?.UpdateTimestamp ?? DateTime.MinValue,
            ResourceName =
                configAuditReportCr.Metadata.Labels != null &&
                configAuditReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.name",
                    out string? resourceName
                ) ? resourceName : string.Empty,
            ResourceNamespace =
                configAuditReportCr.Metadata.Labels != null &&
                configAuditReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.namespace",
                    out string? resourceNamespace
                ) ? resourceNamespace : string.Empty,
            ResourceKind =
                configAuditReportCr.Metadata.Labels != null &&
                configAuditReportCr.Metadata.Labels.TryGetValue(
                    "trivy-operator.resource.kind",
                    out string? resourceKind
                ) ? resourceKind : string.Empty,
            CriticalCount = configAuditReportCr.Report?.Summary?.CriticalCount ?? 0,
            HighCount = configAuditReportCr.Report?.Summary?.HighCount ?? 0,
            MediumCount = configAuditReportCr.Report?.Summary?.MediumCount ?? 0,
            LowCount = configAuditReportCr.Report?.Summary?.LowCount ?? 0,
            Details = [.. configAuditReportDetailDtos,],
        };

        return configAuditReportDto;
    }

    public static IList<ConfigAuditReportDenormalizedDto> ToConfigAuditReportDetailDenormalizedDtos(
        this ConfigAuditReportCr configAuditReportCr
    )
    {
        if (configAuditReportCr is null)
        {
            throw new ArgumentNullException(nameof(configAuditReportCr));
        }

        List<ConfigAuditReportDenormalizedDto> configAuditReportDenormalizedDtos = [];
        foreach (Check check in configAuditReportCr.Report?.Checks ?? [])
        {
            ConfigAuditReportDenormalizedDto configAuditReportDenormalizedDto = new()
            {
                Category = check.Category,
                CheckId = check.CheckId,
                Description = check.Description,
                Messages = check.Messages,
                Remediation = check.Remediation,
                SeverityId = (int)check.Severity,
                Success = check.Success,
                Title = check.Title,
                Uid = new Guid(configAuditReportCr?.Metadata?.Uid ?? string.Empty),
                ResourceName =
                    configAuditReportCr?.Metadata?.Labels != null &&
                    configAuditReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.name",
                        out string? resourceName
                    ) ? resourceName : string.Empty,
                ResourceNamespace =
                    configAuditReportCr?.Metadata?.Labels != null &&
                    configAuditReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.namespace",
                        out string? resourceNamespace
                    ) ? resourceNamespace : string.Empty,
                ResourceKind =
                    configAuditReportCr?.Metadata?.Labels != null &&
                    configAuditReportCr.Metadata.Labels.TryGetValue(
                        "trivy-operator.resource.kind",
                        out string? resourceKind
                    ) ? resourceKind : string.Empty,
            };
            configAuditReportDenormalizedDtos.Add(configAuditReportDenormalizedDto);
        }

        return configAuditReportDenormalizedDtos;
    }
}
