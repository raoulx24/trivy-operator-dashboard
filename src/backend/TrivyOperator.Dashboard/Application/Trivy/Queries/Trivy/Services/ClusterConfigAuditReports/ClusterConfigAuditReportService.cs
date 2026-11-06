using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterConfigAuditReports.Abstractions;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Shared;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterConfigAuditReports;

public class ClusterConfigAuditReportService(
    IResourceProvider<ClusterConfigAuditReport, Uid> resourceProvider
) : QueryService, IClusterConfigAuditReportService
{
    public async Task<QueryResponse<IEnumerable<ClusterConfigAuditReportDto>>> GetClusterConfigAuditReportDtos(
            string? excludedSeverities = null,
            CancellationToken ctx = default)
    {
        QueryResponse<IReadOnlyList<ClusterConfigAuditReport>> result =
            await TrivyQuerySupport.GetResources(resourceProvider, null, excludedSeverities, ctx);

        return new QueryResponse<IEnumerable<ClusterConfigAuditReportDto>>(
            result.Payload.Select(static x => x.ToClusterDto()),
            result.Error);
    }

    public async Task<ClusterConfigAuditReportDto?> GetClusterConfigAuditReportDtoByUid(
            string uid,
            CancellationToken ctx = default)
    {
        ClusterConfigAuditReport? report =
            await resourceProvider.GetResource(new Uid(uid), ctx);

        return report?.ToClusterDto();
    }

    public async Task<IEnumerable<ClusterConfigAuditReportDenormalizedDto>> GetClusterConfigAuditReportDenormalizedDtos(
            CancellationToken ctx = default)
    {
        IReadOnlyList<ClusterConfigAuditReport> result =
            await TrivyQuerySupport.GetResources(resourceProvider, null, ctx);

        return result.SelectMany(static report => report.ToClusterDenormalizedDtos());
    }
}
