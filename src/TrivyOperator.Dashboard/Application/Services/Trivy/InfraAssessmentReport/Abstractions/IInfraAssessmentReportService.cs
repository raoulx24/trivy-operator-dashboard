using TrivyOperator.Dashboard.Application.Models;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.InfraAssessmentReport.Abstractions;

public interface IInfraAssessmentReportService
{
    Task<IEnumerable<string>> GetActiveNamespaces();
    Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetInfraAssessmentReportDenormalizedDtos(string? namespaceName = null);
    Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(Guid uid);
    Task<IEnumerable<InfraAssessmentReportDto>> GetInfraAssessmentReportDtos(string? namespaceName = null, IEnumerable<int>? excludedSeverities = null);
}