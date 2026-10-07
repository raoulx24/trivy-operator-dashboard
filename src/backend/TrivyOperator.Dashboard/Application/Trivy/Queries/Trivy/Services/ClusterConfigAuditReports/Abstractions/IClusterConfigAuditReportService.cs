using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterConfigAuditReports.Abstractions;

public interface IClusterConfigAuditReportService
{
    Task<QueryResponse<IEnumerable<ClusterConfigAuditReportDto>>> GetClusterConfigAuditReportDtos(
            string? excludedSeverities = null,
            CancellationToken ctx = default);

    Task<ClusterConfigAuditReportDto?> GetClusterConfigAuditReportDtoByUid(
        string uid,
        CancellationToken ctx = default);

    Task<IEnumerable<ClusterConfigAuditReportDenormalizedDto>> GetClusterConfigAuditReportDenormalizedDtos(
            CancellationToken ctx = default);

}
