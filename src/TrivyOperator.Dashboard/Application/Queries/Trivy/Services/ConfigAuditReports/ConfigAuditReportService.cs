using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ConfigAuditReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.Shared;
using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ConfigAuditReports;

public class ConfigAuditReportService(
    IResourceProvider<ConfigAuditReport, Uid> resourceProvider
) : IConfigAuditReportService
{
    public async Task<QueryResponse<IEnumerable<ConfigAuditReportDto>>> GetConfigAuditReportDtos(
            string? namespaceName = null,
            string? excludedSeverities = null,
            CancellationToken ctx = default)
    {
        QueryResponse<IReadOnlyList<ConfigAuditReport>> result =
            await TrivyQuerySupport.GetResources(resourceProvider, namespaceName, excludedSeverities, ctx);

        return new QueryResponse<IEnumerable<ConfigAuditReportDto>>(
            result.Payload.Select(static x => x.ToDto()),
            result.Error);
    }

    public async Task<ConfigAuditReportDto?> GetConfigAuditReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ConfigAuditReport? report =
            await resourceProvider.GetResource(new Uid(uid), ctx);

        return report?.ToDto();
    }

    public async Task<IEnumerable<ConfigAuditReportDenormalizedDto>> GetConfigAuditReportDenormalizedDtos(
            string? namespaceName = null,
            CancellationToken ctx = default)
    {
        IReadOnlyList<ConfigAuditReport> result =
            await TrivyQuerySupport.GetResources(resourceProvider, namespaceName, ctx);

        return result.SelectMany(report => report.ToDenormalizedDtos());
    }
}
