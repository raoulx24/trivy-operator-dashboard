using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports;

public class RbacAssessmentReportNullService : IRbacAssessmentReportService
{
    public Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetRbacAssessmentReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    ) =>
        Task.FromResult<IEnumerable<RbacAssessmentReportDenormalizedDto>>([]);

    public Task<IEnumerable<RbacAssessmentReportDto>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        IReadOnlySet<int>? excludedSeverities = null,
        CancellationToken ctx = default
    ) =>
        Task.FromResult<IEnumerable<RbacAssessmentReportDto>>([]);
}
