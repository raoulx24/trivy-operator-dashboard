using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ExposedSecretReports.Abstractions;

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

    Task<IEnumerable<ExposedSecretReportImageDto>>
        GetExposedSecretReportImageDtos(
            string? namespaceName = null,
            IReadOnlySet<int>? includedSeverities = null,
            CancellationToken ctx = default);
}
