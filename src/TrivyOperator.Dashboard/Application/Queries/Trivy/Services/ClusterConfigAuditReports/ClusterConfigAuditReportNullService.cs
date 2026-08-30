using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterConfigAuditReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterConfigAuditReports;

public class ClusterConfigAuditReportNullService : IClusterConfigAuditReportService
{
    public Task<QueryResponse<IEnumerable<ClusterConfigAuditReportDto>>> GetClusterConfigAuditReportDtos(
            string? excludedSeverities = null,
            CancellationToken ctx = default)
        => Task.FromResult(new QueryResponse<IEnumerable<ClusterConfigAuditReportDto>>([], null));

    public Task<ClusterConfigAuditReportDto?> GetClusterConfigAuditReportDtoByUid(
            string uid,
            CancellationToken ctx = default)
        => Task.FromResult<ClusterConfigAuditReportDto?>(null);

    public Task<IEnumerable<ClusterConfigAuditReportDenormalizedDto>> GetClusterConfigAuditReportDenormalizedDtos(
            CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<ClusterConfigAuditReportDenormalizedDto>>([]);

}
