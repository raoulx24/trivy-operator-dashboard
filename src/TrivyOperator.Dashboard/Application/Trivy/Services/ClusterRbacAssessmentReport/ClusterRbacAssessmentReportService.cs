using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.TrivyOld;
using TrivyOperator.Dashboard.Domain.TrivyOld.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.InMemoryOld.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport;

public class ClusterRbacAssessmentReportService(IConcurrentDictionaryCache<OldClusterRbacAssessmentReportCr> cache)
    : IClusterRbacAssessmentReportService
{
    public Task<IEnumerable<ClusterRbacAssessmentReportDto>> GetClusterRbacAssessmentReportDtos()
    {
        IEnumerable<OldClusterRbacAssessmentReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterRbacAssessmentReportDto> values =
            cachedValues.Select(x => x.ToClusterRbacAssessmentReportDto());

        return Task.FromResult(values);
    }

    public Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>
        GetClusterRbacAssessmentReportDenormalizedDtos()
    {
        IEnumerable<OldClusterRbacAssessmentReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterRbacAssessmentReportDenormalizedDto> values =
            cachedValues.SelectMany(cr => cr.ToClusterRbacAssessmentReportDenormalizedDtos());

        return Task.FromResult(values);
    }

    public Task<IEnumerable<ClusterRbacAssessmentReportSummaryDto>> GetClusterRbacAssessmentReportSummaryDtos()
    {
        int[] allSeverities = [.. Enum.GetValues<TrivySeverity>().Cast<int>().Where(x => x < 4),];

        IEnumerable<OldClusterRbacAssessmentReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterRbacAssessmentReportSummaryDto> actualValues = cachedValues
            .SelectMany(crar => crar.Report?.Checks ?? [])
            .GroupBy(key => key.Severity)
            .Select(group => new ClusterRbacAssessmentReportSummaryDto
                {
                    SeverityId = (int)group.Key,
                    TotalCount = group.Count(),
                    DistinctCount = group.Select(x => x.CheckId).Distinct().Count(),
                }
            );
        IEnumerable<ClusterRbacAssessmentReportSummaryDto> values = allSeverities.GroupJoin(
            actualValues,
            left => left,
            right => right.SeverityId,
            (left, group) =>
            {
                ClusterRbacAssessmentReportSummaryDto[] groupArray = [.. group,];
                return new ClusterRbacAssessmentReportSummaryDto
                {
                    SeverityId = left,
                    TotalCount = groupArray.FirstOrDefault()?.TotalCount ?? 0,
                    DistinctCount = groupArray.FirstOrDefault()?.DistinctCount ?? 0,
                };
            }
        );

        return Task.FromResult(values);
    }
}
