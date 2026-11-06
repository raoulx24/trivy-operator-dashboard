using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterRbacAssessmentReports.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterRbacAssessmentReports;

public class ClusterRbacAssessmentReportNullService : QueryNullService, IClusterRbacAssessmentReportService
{
    public Task<ClusterRbacAssessmentReportDto?> GetClusterRbacAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default) 
        => Task.FromResult<ClusterRbacAssessmentReportDto?>(null);

    public Task<IEnumerable<ClusterRbacAssessmentReportDto>>
        GetClusterRbacAssessmentReportDtos(CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<ClusterRbacAssessmentReportDto>>([]);

    public Task<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>
        GetClusterRbacAssessmentReportDenormalizedDtos(CancellationToken ctx = default) 
        => Task.FromResult<IEnumerable<ClusterRbacAssessmentReportDenormalizedDto>>([]);
}
