using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ExposedSecretReports.Abstractions;

public interface IExposedSecretReportService
{
    Task<IEnumerable<ExposedSecretReportDto>> GetExposedSecretReportDtos(
        string? namespaceName = null,
        CancellationToken ctx = default);

    Task<ExposedSecretReportDto?> GetExposedSecretReportDtoByUid(
        string uid,
        CancellationToken ctx = default);

    Task<IEnumerable<ExposedSecretReportDenormalizedDto>>
        GetExposedSecretReportDenormalizedDtos(
            string? namespaceName = null,
            CancellationToken ctx = default);

    Task<QueryResponse<IEnumerable<ExposedSecretReportImageDto>>>
        GetExposedSecretReportImageDtos(
            string? namespaceName = null,
            string? excludedSeverities = null,
            CancellationToken ctx = default);
    
    Task<string[]> GetActiveNamespaces(CancellationToken ctx = default);
}
