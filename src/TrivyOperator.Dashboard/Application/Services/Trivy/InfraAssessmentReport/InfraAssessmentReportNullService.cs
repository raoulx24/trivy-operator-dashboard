using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.InfraAssessmentReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.InfraAssessmentReport;

public class InfraAssessmentReportNullService : IInfraAssessmentReportService
{
    public Task<IEnumerable<InfraAssessmentReportDto>> GetInfraAssessmentReportDtos(
        string? namespaceName = null, 
        IEnumerable<int>? excludedSeverities = null) => Task.FromResult<IEnumerable<InfraAssessmentReportDto>>([]);

    public Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(Guid uid) =>
        Task.FromResult<InfraAssessmentReportDto?>(null);

    public Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetInfraAssessmentReportDenormalizedDtos(
        string? namespaceName = null) => Task.FromResult<IEnumerable<InfraAssessmentReportDenormalizedDto>>([]);

    public Task<IEnumerable<string>> GetActiveNamespaces() => Task.FromResult<IEnumerable<string>>([]);

}
