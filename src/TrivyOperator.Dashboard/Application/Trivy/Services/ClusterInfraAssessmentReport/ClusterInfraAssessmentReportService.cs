using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport.Abstractions;
using TrivyOperator.Dashboard.Domain.Trivy.ClusterInfraAssessmentReport;
using TrivyOperator.Dashboard.Infrastructure.Caching.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport;

public class ClusterInfraAssessmentReportService(IConcurrentDictionaryCache<ClusterInfraAssessmentReportCr> cache)
    : IClusterInfraAssessmentReportService
{
    public Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos()
    {
        IEnumerable<ClusterInfraAssessmentReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];

        IEnumerable<ClusterInfraAssessmentReportDto> values =
            cachedValues.Select(cr => cr.ToClusterInfraAssessmentReportDto());

        return Task.FromResult(values);
    }

    public Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(Guid uid)
    {
        IEnumerable<ClusterInfraAssessmentReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];

        ClusterInfraAssessmentReportDto? result = cachedValues.Select(cr => cr.ToClusterInfraAssessmentReportDto())
            .FirstOrDefault(dto => dto.Uid == uid);

        return Task.FromResult<ClusterInfraAssessmentReportDto?>(null);
    }


    public Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>
        GetClusterInfraAssessmentReportDenormalizedDtos()
    {
        IEnumerable<ClusterInfraAssessmentReportCr> cachedValues = [.. cache.SelectMany(kvp => kvp.Value.Values),];
        IEnumerable<ClusterInfraAssessmentReportDenormalizedDto> values =
            cachedValues.SelectMany(car => car.ToClusterInfraAssessmentReportDetailDenormalizedDtos());

        return Task.FromResult(values);
    }
}
