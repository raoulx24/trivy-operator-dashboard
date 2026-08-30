
using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ConfigAuditReports.Abstractions;

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
}
