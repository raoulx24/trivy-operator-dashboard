using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterInfraAssessmentReports;

public class ClusterInfraAssessmentReportNullService : QueryNullService, IClusterInfraAssessmentReportService
{
    public Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos(CancellationToken ctx = default) =>
        Task.FromResult<IEnumerable<ClusterInfraAssessmentReportDto>>([]);

    public Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(string uid, CancellationToken ctx = default) =>
        Task.FromResult<ClusterInfraAssessmentReportDto?>(null);

    public Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>
        GetClusterInfraAssessmentReportDenormalizedDtos(CancellationToken ctx = default) =>
        Task.FromResult<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>([]);
}
