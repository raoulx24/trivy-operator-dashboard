using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterRbacAssessmentReports;

public class ClusterRbacAssessmentReportNullService : IClusterRbacAssessmentReportService
{
    public Task<IEnumerable<ClusterRbacAssessmentReportDto>>
        GetClusterRbacAssessmentReportDtos(CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<ClusterRbacAssessmentReportDto>>([]);

    public Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>
        GetClusterRbacAssessmentReportDenormalizedDtos(CancellationToken ctx = default) 
        => Task.FromResult<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>([]);
}
