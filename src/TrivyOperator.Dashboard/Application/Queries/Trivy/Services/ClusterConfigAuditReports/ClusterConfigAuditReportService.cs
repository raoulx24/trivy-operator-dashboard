using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterConfigAuditReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.Shared;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterConfigAuditReports;

public class ClusterConfigAuditReportService(
    IResourceProvider<ConfigAuditReport, Uid> resourceProvider
) : IClusterConfigAuditReportService
{
    public async Task<QueryResponse<IEnumerable<ClusterConfigAuditReportDto>>> GetClusterConfigAuditReportDtos(
            string? excludedSeverities = null,
            CancellationToken ctx = default)
    {
        QueryResponse<IReadOnlyList<ConfigAuditReport>> result =
            await TrivyQuerySupport.GetResources(resourceProvider, null, excludedSeverities, ctx);

        return new QueryResponse<IEnumerable<ClusterConfigAuditReportDto>>(
            result.Payload.Select(static x => x.ToClusterDto()),
            result.Error);
    }

    public async Task<ClusterConfigAuditReportDto?> GetClusterConfigAuditReportDtoByUid(
            string uid,
            CancellationToken ctx = default)
    {
        ConfigAuditReport? report =
            await resourceProvider.GetResource(new Uid(uid), ctx);

        return report?.ToClusterDto();
    }

    public async Task<IEnumerable<ClusterConfigAuditReportDenormalizedDto>> GetClusterConfigAuditReportDenormalizedDtos(
            CancellationToken ctx = default)
    {
        IReadOnlyList<ConfigAuditReport> result =
            await TrivyQuerySupport.GetResources(resourceProvider, null, ctx);

        return result.SelectMany(static report => report.ToClusterDenormalizedDtos());
    }
}
