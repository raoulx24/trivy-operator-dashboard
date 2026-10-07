using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ConfigAuditReports.Abstractions;

public interface IConfigAuditReportService
{
    Task<IEnumerable<ConfigAuditReportDenormalizedDto>> GetConfigAuditReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    );

    Task<ConfigAuditReportDto?> GetConfigAuditReportDtoByUid(string uid, CancellationToken ctx = default);

    Task<QueryResponse<IEnumerable<ConfigAuditReportDto>>> GetConfigAuditReportDtos(
        string? namespaceName = null,
        string? excludedSeverities = null,
        CancellationToken ctx = default
    );
    
    Task<string[]> GetActiveNamespaces(CancellationToken ctx = default);
}
