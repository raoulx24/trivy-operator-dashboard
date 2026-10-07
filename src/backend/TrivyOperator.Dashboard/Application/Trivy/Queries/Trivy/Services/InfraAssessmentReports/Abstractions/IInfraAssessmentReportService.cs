using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;

public interface IInfraAssessmentReportService
{
    Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetInfraAssessmentReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    );

    Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(string uid, CancellationToken ctx = default);

    Task<QueryResponse<IEnumerable<InfraAssessmentReportDto>>> GetInfraAssessmentReportDtos(
        string? namespaceName = null,
        string? excludedSeverities = null,
        CancellationToken ctx = default
    );
    
    Task<string[]> GetActiveNamespaces(CancellationToken ctx = default);
}
