using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterInfraAssessmentReports;

public class ClusterInfraAssessmentReportNullService : IClusterInfraAssessmentReportService
{
    public Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos(CancellationToken ctx = default) =>
        Task.FromResult<IEnumerable<ClusterInfraAssessmentReportDto>>([]);

    public Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(string uid, CancellationToken ctx = default) =>
        Task.FromResult<ClusterInfraAssessmentReportDto?>(null);

    public Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>
        GetClusterInfraAssessmentReportDenormalizedDtos(CancellationToken ctx = default) =>
        Task.FromResult<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>([]);
}
