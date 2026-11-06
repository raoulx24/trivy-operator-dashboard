using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;

public interface IClusterInfraAssessmentReportService : IQueryService
{
    Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos(CancellationToken ctx = default);
    Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(string uid, CancellationToken ctx = default);
    Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetClusterInfraAssessmentReportDenormalizedDtos(CancellationToken ctx = default);
}
