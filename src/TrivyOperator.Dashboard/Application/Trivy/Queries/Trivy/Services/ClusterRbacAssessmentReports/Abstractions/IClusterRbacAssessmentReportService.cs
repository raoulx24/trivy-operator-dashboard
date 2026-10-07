using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;

public interface IClusterRbacAssessmentReportService
{
    Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>> GetClusterRbacAssessmentReportDenormalizedDtos(
        CancellationToken ctx = default);
    Task<ClusterRbacAssessmentReportDto?> GetClusterRbacAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default);
    Task<IEnumerable<ClusterRbacAssessmentReportDto>> GetClusterRbacAssessmentReportDtos(CancellationToken ctx = default);
}
