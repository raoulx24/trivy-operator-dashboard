using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.InfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.TrivyOld;
using TrivyOperator.Dashboard.Domain.TrivyOld.InfraAssessmentReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.InfraAssessmentReport;

public class InfraAssessmentReportService(IConcurrentDictionaryCache<InfraAssessmentReportCr> cache)
    : IInfraAssessmentReportService
{
    public Task<IEnumerable<InfraAssessmentReportDto>> GetInfraAssessmentReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null
    )
    {
        excludedSeverities ??= [];
        int[] excludedSeveritiesArray = [.. excludedSeverities,];
        bool hasExcludedSeverities = excludedSeveritiesArray.Length != 0;
        int[] includedSeverities = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Except(excludedSeveritiesArray),];

        IEnumerable<InfraAssessmentReportCr> cachedValues =
        [
            .. cache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];

        IEnumerable<InfraAssessmentReportDto> values = cachedValues.Select(cr => cr.ToInfraAssessmentReportDto())
            .Select(dto =>
                {
                    dto.Details =
                    [
                        .. dto.Details.Join(
                            includedSeverities,
                            vulnerability => vulnerability.SeverityId,
                            id => id,
                            (vulnerability, _) => vulnerability
                        ),
                    ];
                    return dto;
                }
            )
            .Where(dto => !hasExcludedSeverities || dto.Details.Length != 0);

        return Task.FromResult(values);
    }

    public Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(Guid uid)
    {
        string[] namespaceNames = [.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),];

        foreach (string namespaceName in namespaceNames)
        {
            if (cache.TryGetValue(
                    namespaceName,
                    out ConcurrentDictionary<string, InfraAssessmentReportCr>? InfraAssessmentReportCrs
                ))
            {
                if (InfraAssessmentReportCrs.TryGetValue(
                        uid.ToString(),
                        out InfraAssessmentReportCr? InfraAssessmentReportCr
                    ))
                {
                    return Task.FromResult<InfraAssessmentReportDto?>(
                        InfraAssessmentReportCr.ToInfraAssessmentReportDto()
                    );
                }
            }
        }

        return Task.FromResult<InfraAssessmentReportDto?>(null);
    }


    public Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetInfraAssessmentReportDenormalizedDtos(
        string? namespaceName = null
    )
    {
        IEnumerable<InfraAssessmentReportCr> cachedValues =
        [
            .. cache.Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
                .SelectMany(kvp => kvp.Value.Values),
        ];
        IEnumerable<InfraAssessmentReportDenormalizedDto> values =
            cachedValues.SelectMany(car => car.ToInfraAssessmentReportDetailDenormalizedDtos());

        return Task.FromResult(values);
    }

    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult<IEnumerable<string>>([.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),]);
}
