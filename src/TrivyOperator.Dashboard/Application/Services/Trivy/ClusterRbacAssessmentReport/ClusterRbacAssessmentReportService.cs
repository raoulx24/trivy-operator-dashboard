﻿using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterRbacAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterRbacAssessmentReport;
using TrivyOperator.Dashboard.Infrastructure.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.ClusterRbacAssessmentReport;

public class ClusterRbacAssessmentReportService(IConcurrentCache<string, IList<ClusterRbacAssessmentReportCr>> cache)
    : IClusterRbacAssessmentReportService
{
    public Task<IList<ClusterRbacAssessmentReportDto>> GetClusterRbacAssessmentReportDtos()
    {
        List<ClusterRbacAssessmentReportDto> result = cache.SelectMany(kvp => kvp.Value)
            .Select(cr => cr.ToClusterRbacAssessmentReportDto())
            .ToList();

        return Task.FromResult<IList<ClusterRbacAssessmentReportDto>>(result);
    }

    public Task<IList<ClusterRbacAssessmentReportDenormalizedDto>> GetClusterRbacAssessmentReportDenormalizedDtos()
    {
        List<ClusterRbacAssessmentReportDenormalizedDto> result = cache.SelectMany(kvp => kvp.Value)
            .SelectMany(cr => cr.ToClusterRbacAssessmentReportDenormalizedDtos())
            .ToList();

        return Task.FromResult<IList<ClusterRbacAssessmentReportDenormalizedDto>>(result);
    }

    public Task<IList<ClusterRbacAssessmentReportSummaryDto>> GetClusterRbacAssessmentReportSummaryDtos()
    {
        int[] allSeverities = Enum.GetValues(typeof(TrivySeverity)).Cast<int>().Where(x => x < 4).ToArray();

        IEnumerable<ClusterRbacAssessmentReportSummaryDto> actualValues = cache.SelectMany(kvp => kvp.Value)
            //.Where(crar => crar.Report != null)
            .SelectMany(crar => crar.Report?.Checks ?? Enumerable.Empty<Check>())
            .GroupBy(key => key.Severity)
            .Select(
                group => new ClusterRbacAssessmentReportSummaryDto
                {
                    SeverityId = (int)group.Key,
                    TotalCount = group.Count(),
                    DistinctCount = group.Select(x => x.CheckId).Distinct().Count(),
                });

        List<ClusterRbacAssessmentReportSummaryDto> result = allSeverities.GroupJoin(
                actualValues,
                left => left,
                right => right.SeverityId,
                (left, group) =>
                {
                    ClusterRbacAssessmentReportSummaryDto[] groupArray = group.ToArray();
                    return new ClusterRbacAssessmentReportSummaryDto
                    {
                        SeverityId = left,
                        TotalCount = groupArray.FirstOrDefault()?.TotalCount ?? 0,
                        DistinctCount = groupArray.FirstOrDefault()?.DistinctCount ?? 0,
                    };
                })
            .ToList();

        return Task.FromResult<IList<ClusterRbacAssessmentReportSummaryDto>>(result);
    }
}
