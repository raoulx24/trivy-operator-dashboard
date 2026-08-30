using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.RbacAssessmentReports;

public class RbacAssessmentReportNullService : IRbacAssessmentReportService
{
    public Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetRbacAssessmentReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<RbacAssessmentReportDenormalizedDto>>([]);

    public Task<QueryResponse<IEnumerable<RbacAssessmentReportDto>>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        string?  excludedSeverities = null,
        CancellationToken ctx = default)
        => Task.FromResult(new QueryResponse<IEnumerable<RbacAssessmentReportDto>>([], null));
    
    public Task<RbacAssessmentReportDto?> GetRbacAssessmentReportDtoByUid(
        string uid,
        CancellationToken ctx = default) 
        => Task.FromResult<RbacAssessmentReportDto?>(null);
}
