using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;

public interface IClusterRbacAssessmentReportService
{
    Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>> GetClusterRbacAssessmentReportDenormalizedDtos(CancellationToken ctx = default);
    Task<IEnumerable<ClusterRbacAssessmentReportDto>> GetClusterRbacAssessmentReportDtos(CancellationToken ctx = default);
}
