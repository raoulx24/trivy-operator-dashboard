using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.InfraAssessmentReport.Abstractions;

public interface IInfraAssessmentReportService
{
    Task<IEnumerable<string>> GetActiveNamespaces();
    Task<IEnumerable<InfraAssessmentReportDenormalizedDto>> GetInfraAssessmentReportDenormalizedDtos(string? namespaceName = null);
    Task<InfraAssessmentReportDto?> GetInfraAssessmentReportDtoByUid(Guid uid);
    Task<IEnumerable<InfraAssessmentReportDto>> GetInfraAssessmentReportDtos(string? namespaceName = null, IEnumerable<int>? excludedSeverities = null);
}