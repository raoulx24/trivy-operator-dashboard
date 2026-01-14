using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ConfigAuditReport.Abstractions;

public interface IConfigAuditReportService
{
    Task<IEnumerable<ConfigAuditReportDenormalizedDto>> GetConfigAuditReportDenormalizedDtos(
        string? namespaceName = null
    );

    Task<ConfigAuditReportDto?> GetConfigAuditReportDtoByUid(Guid uid);

    Task<IEnumerable<ConfigAuditReportDto>> GetConfigAuditReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null
    );

    Task<IEnumerable<string>> GetActiveNamespaces();
    Task<IEnumerable<ConfigAuditReportSummaryDto>> GetConfigAuditReportSummaryDtos();
}
