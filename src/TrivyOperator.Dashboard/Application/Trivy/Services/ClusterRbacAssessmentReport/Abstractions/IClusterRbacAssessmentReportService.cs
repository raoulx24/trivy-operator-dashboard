using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport.Abstractions;

public interface IClusterRbacAssessmentReportService
{
    Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>> GetClusterRbacAssessmentReportDenormalizedDtos();
    Task<IEnumerable<ClusterRbacAssessmentReportDto>> GetClusterRbacAssessmentReportDtos();
    Task<IEnumerable<ClusterRbacAssessmentReportSummaryDto>> GetClusterRbacAssessmentReportSummaryDtos();
}
