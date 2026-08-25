using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.InfraAssessmentReports;

public class InfraAssessmentReportNullService : IInfraAssessmentReportService
{
    public Task<IEnumerable<InfraAssessmentReportDto>> GetInfraAssessmentReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null,
        CancellationToken ctx = default
    ) => Task.FromResult<IEnumerable<InfraAssessmentReportDto>>([]);

    public Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(string uid, CancellationToken ctx = default) =>
        Task.FromResult<InfraAssessmentReportDto?>(null);

    public Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetInfraAssessmentReportDenormalizedDtos(
        string? namespaceName = null,
        CancellationToken ctx = default
    ) => Task.FromResult<IEnumerable<InfraAssessmentReportDenormalizedDto>>([]);
}
