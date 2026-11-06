using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;

public interface IClusterRbacAssessmentReportService : IQueryService
{
    Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>> GetClusterRbacAssessmentReportDenormalizedDtos(
        CancellationToken ctx = default);
    Task<ClusterRbacAssessmentReportDto?> GetClusterRbacAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default);
    Task<IEnumerable<ClusterRbacAssessmentReportDto>> GetClusterRbacAssessmentReportDtos(CancellationToken ctx = default);
}
