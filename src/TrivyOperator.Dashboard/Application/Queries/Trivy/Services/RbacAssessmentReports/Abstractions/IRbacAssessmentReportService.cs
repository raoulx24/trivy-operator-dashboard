using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;

public interface IRbacAssessmentReportService
{
    Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetRbacAssessmentReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    );

    Task<IEnumerable<RbacAssessmentReportDto>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        IReadOnlySet<int>? excludedSeverities = null,
        CancellationToken ctx = default
    );
}
