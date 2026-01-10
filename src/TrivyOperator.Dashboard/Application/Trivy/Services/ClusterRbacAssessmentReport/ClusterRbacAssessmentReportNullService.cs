using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterRbacAssessmentReport;

public class ClusterRbacAssessmentReportNullService : IClusterRbacAssessmentReportService
{
    public Task<IEnumerable<ClusterRbacAssessmentReportDto>> GetClusterRbacAssessmentReportDtos() =>
        Task.FromResult<IEnumerable<ClusterRbacAssessmentReportDto>>([]);

    public Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>> GetClusterRbacAssessmentReportDenormalizedDtos() =>
        Task.FromResult<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>([]);

    public Task<IEnumerable<ClusterRbacAssessmentReportSummaryDto>> GetClusterRbacAssessmentReportSummaryDtos() =>
        Task.FromResult<IEnumerable<ClusterRbacAssessmentReportSummaryDto>>([]);
}
