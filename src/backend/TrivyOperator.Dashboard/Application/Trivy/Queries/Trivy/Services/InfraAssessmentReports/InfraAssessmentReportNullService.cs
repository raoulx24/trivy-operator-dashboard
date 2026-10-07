using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.InfraAssessmentReports;

public class InfraAssessmentReportNullService : IInfraAssessmentReportService
{
    public Task<QueryResponse<IEnumerable<InfraAssessmentReportDto>>> GetInfraAssessmentReportDtos(
        string? namespaceName = null,
        string? excludedSeverities = null,
        CancellationToken ctx = default
    ) => Task.FromResult(new QueryResponse<IEnumerable<InfraAssessmentReportDto>>([], null));

    public Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(string uid, CancellationToken ctx = default) =>
        Task.FromResult<InfraAssessmentReportDto?>(null);

    public Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetInfraAssessmentReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    ) => Task.FromResult<IEnumerable<InfraAssessmentReportDenormalizedDto>>([]);
    
    public Task<string[]> GetActiveNamespaces(CancellationToken ctx = default) => Task.FromResult<string[]>([]);
}
