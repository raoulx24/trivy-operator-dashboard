using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.Trivy.RbacAssessmentReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport;

public class RbacAssessmentReportService(IConcurrentDictionaryCache<RbacAssessmentReportCr> cache)
    : IRbacAssessmentReportService
{
    public Task<IEnumerable<RbacAssessmentReportDto>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null)
    {
        excludedSeverities ??= [];
        int[] excludedSeveritiesArray = [.. excludedSeverities,];
        int[] includedSeverities = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Except(excludedSeveritiesArray),];

        IEnumerable<RbacAssessmentReportCr> cachedValues = [.. cache
            .Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
            .SelectMany(kvp => kvp.Value.Values),];

        IEnumerable<RbacAssessmentReportDto> dtos = cachedValues.Select(cr => cr.ToRbacAssessmentReportDto())
                    .Select(
                        dto =>
                        {
                            dto.Details = dto.Details.Join(
                                    includedSeverities,
                                    vulnerability => vulnerability.SeverityId,
                                    id => id,
                                    (vulnerability, _) => vulnerability)
                                .ToArray();
                            return dto;
                        })
                    .Where(dto => excludedSeveritiesArray.Length == 0 || dto.Details.Length != 0);

        return Task.FromResult(dtos);
    }

    public Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetRbacAssessmentReportDenormalizedDtos(
        string? namespaceName = null)
    {
        IEnumerable<RbacAssessmentReportCr> cachedValues = [.. cache
            .Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
            .SelectMany(kvp => kvp.Value.Values),];

        IEnumerable<RbacAssessmentReportDenormalizedDto> result = cachedValues
            .SelectMany(cr => cr.ToRbacAssessmentReportDenormalizedDtos());

        return Task.FromResult(result);
    }

    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult<IEnumerable<string>>([.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),]);
}
