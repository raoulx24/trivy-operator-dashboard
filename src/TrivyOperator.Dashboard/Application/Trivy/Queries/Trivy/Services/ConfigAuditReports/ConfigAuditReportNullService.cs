using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ConfigAuditReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ConfigAuditReports;

public class ConfigAuditReportNullService : IConfigAuditReportService
{
    public Task<QueryResponse<IEnumerable<ConfigAuditReportDto>>> GetConfigAuditReportDtos(
        string? namespaceName = null,
        string? excludedSeverities = null,
        CancellationToken ctx = default
    ) => Task.FromResult(new QueryResponse<IEnumerable<ConfigAuditReportDto>>([], null));

    public Task<ConfigAuditReportDto?> GetConfigAuditReportDtoByUid(string uid, CancellationToken ctx = default) =>
        Task.FromResult<ConfigAuditReportDto?>(null);

    public Task<IEnumerable<ConfigAuditReportDenormalizedDto>> GetConfigAuditReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    ) => Task.FromResult<IEnumerable<ConfigAuditReportDenormalizedDto>>([]);
    
    public Task<string[]> GetActiveNamespaces(CancellationToken ctx = default) => Task.FromResult<string[]>([]);
}
