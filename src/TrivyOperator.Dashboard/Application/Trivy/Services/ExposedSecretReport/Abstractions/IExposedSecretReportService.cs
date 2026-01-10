using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ExposedSecretReport.Abstractions;

public interface IExposedSecretReportService
{
    Task<IEnumerable<string>> GetActiveNamespaces();
    Task<IEnumerable<ExposedSecretReportDenormalizedDto>> GetExposedSecretDenormalizedDtos(string? namespaceName = null);

    Task<IEnumerable<ExposedSecretReportDto>> GetExposedSecretReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null);

    public Task<IEnumerable<ExposedSecretReportImageDto>> GetExposedSecretReportImageDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null);

    Task<ExposedSecretReportImageDto?> GetExposedSecretReportImageDtoByDigestNamespace(string digest, string namespaceName);

    public Task<IEnumerable<EsSeveritiesByNsSummaryDto>> GetExposedSecretReportSummaryDtos();
}
