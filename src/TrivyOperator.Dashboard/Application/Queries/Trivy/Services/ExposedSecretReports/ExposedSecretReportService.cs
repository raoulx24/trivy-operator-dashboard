using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Mappers;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports.Abstractions;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Shared;
using TrivyOperator.Dashboard.Domain.Shared.Stores.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports;

public class ExposedSecretReportService(
    IResourceProvider<ExposedSecretReport, Digest> resourceProvider,
    ILogger<ExposedSecretReportService> logger)
    : IExposedSecretReportService
{
    public async Task<IEnumerable<ExposedSecretReportDto>> GetExposedSecretReportDtos(
        string? namespaceName = null,
        CancellationToken ctx = default)
    {
        IReadOnlyList<ExposedSecretReport> result = 
            await TrivyQuerySupport.GetResources(resourceProvider, namespaceName, ctx);
        
        return result.SelectMany(static x => x.ToDtos());
    }

    public async Task<ExposedSecretReportDto?> GetExposedSecretReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
    {
        ExposedSecretReport? report = 
            await TrivyQuerySupport.GetImageDigestReportDtoByUid(resourceProvider, uid, ctx);

        return report?.ToDtos().FirstOrDefault(x => x.Uid == uid.ToLowerInvariant());
    }

    public async Task<IEnumerable<ExposedSecretReportDenormalizedDto>>
        GetExposedSecretReportDenormalizedDtos(
            string? namespaceName = null,
            CancellationToken ctx = default)
    {
        IReadOnlyList<ExposedSecretReport> reports =
            await TrivyQuerySupport.GetResources(resourceProvider, namespaceName, ctx);

        return reports.SelectMany(static x => x.ToDenormalizedDtos());
    }

    public async Task<QueryResponse<IEnumerable<ExposedSecretReportImageDto>>>
        GetExposedSecretReportImageDtos(
            string? namespaceName = null,
            string? excludedSeverities = null,
            CancellationToken ctx = default)
    {
        QueryResponse<IReadOnlyList<ExposedSecretReport>> result = 
            await TrivyQuerySupport.GetResources(resourceProvider, namespaceName, excludedSeverities, ctx);
        
        return new QueryResponse<IEnumerable<ExposedSecretReportImageDto>>(
            result.Payload.Select(static x => x.ToDto()),
            result.Error);
    }
}
