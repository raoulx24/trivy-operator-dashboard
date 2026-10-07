using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;

public interface IRbacAssessmentReportService
{
    Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetRbacAssessmentReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    );

    Task<QueryResponse<IEnumerable<RbacAssessmentReportDto>>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        string? excludedSeverities = null,
        CancellationToken ctx = default
    );
    
    Task<RbacAssessmentReportDto?> GetRbacAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default);
    
    Task<string[]> GetActiveNamespaces(CancellationToken ctx = default);
}
