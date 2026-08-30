using TrivyOperator.Dashboard.Application.Queries.Shared;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;

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
}
