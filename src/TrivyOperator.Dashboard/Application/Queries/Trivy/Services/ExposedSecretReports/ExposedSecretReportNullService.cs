using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports;

public class ExposedSecretReportNullService : IExposedSecretReportService
{
    public Task<IEnumerable<ExposedSecretReportDto>> GetExposedSecretReportDtos(
        string? namespaceName = null,
        CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<ExposedSecretReportDto>>([]);

    public Task<ExposedSecretReportDto?> GetExposedSecretReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
        => Task.FromResult<ExposedSecretReportDto?>(null);

    public Task<IEnumerable<ExposedSecretReportDenormalizedDto>>
        GetExposedSecretReportDenormalizedDtos(
            string? namespaceName = null,
            CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<ExposedSecretReportDenormalizedDto>>([]);

    public Task<QueryResponse<IEnumerable<ExposedSecretReportImageDto>>>
        GetExposedSecretReportImageDtos(
            string? namespaceName = null,
            string? excludedSeverities = null,
            CancellationToken ctx = default)
        => Task.FromResult(new QueryResponse<IEnumerable<ExposedSecretReportImageDto>>([], null));
}
