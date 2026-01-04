using System.Collections.Concurrent;
using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterInfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.ClusterInfraAssessmentReport;

public class ClusterInfraAssessmentReportService(IConcurrentDictionaryCache<ClusterInfraAssessmentReportCr> cache) : IClusterInfraAssessmentReportService
{
    public Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null)
    {
        excludedSeverities ??= [];
        int[] excludedSeveritiesArray = [.. excludedSeverities,];
        bool hasExcludedSeverities = excludedSeveritiesArray.Length != 0;
        int[] includedSeverities = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Except(excludedSeveritiesArray),];

        IEnumerable<ClusterInfraAssessmentReportCr> cachedValues = [.. cache
            .Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
            .SelectMany(kvp => kvp.Value.Values),];

        IEnumerable<ClusterInfraAssessmentReportDto> values = cachedValues
            .Select(cr => cr.ToClusterInfraAssessmentReportDto())
            .Select(
                dto =>
                {
                    dto.Details = [.. dto.Details.Join(
                            includedSeverities,
                            vulnerability => vulnerability.SeverityId,
                            id => id,
                            (vulnerability, _) => vulnerability),];
                    return dto;
                })
            .Where(dto => !hasExcludedSeverities || dto.Details.Length != 0);

        return Task.FromResult(values);
    }

    public Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(Guid uid)
    {
        string[] namespaceNames = [.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key)];

        foreach (string namespaceName in namespaceNames)
        {
            if (cache.TryGetValue(namespaceName, out ConcurrentDictionary<string, ClusterInfraAssessmentReportCr>? ClusterInfraAssessmentReportCrs))
            {
                if (ClusterInfraAssessmentReportCrs.TryGetValue(uid.ToString(), out ClusterInfraAssessmentReportCr? ClusterInfraAssessmentReportCr))
                {
                    return Task.FromResult<ClusterInfraAssessmentReportDto?>(ClusterInfraAssessmentReportCr.ToClusterInfraAssessmentReportDto());
                }
            }
        }

        return Task.FromResult<ClusterInfraAssessmentReportDto?>(null);
    }


    public Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetClusterInfraAssessmentReportDenormalizedDtos(
        string? namespaceName = null)
    {
        IEnumerable<ClusterInfraAssessmentReportCr> cachedValues = [.. cache
            .Where(kvp => string.IsNullOrEmpty(namespaceName) || kvp.Key == namespaceName)
            .SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterInfraAssessmentReportDenormalizedDto> values = cachedValues
            .SelectMany(car => car.ToClusterInfraAssessmentReportDetailDenormalizedDtos());

        return Task.FromResult(values);
    }

    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult<IEnumerable<string>>([.. cache.Where(x => !x.Value.IsEmpty).Select(x => x.Key),]);

}
