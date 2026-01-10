using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport.Abstractions;

public interface IRbacAssessmentReportService
{
    Task<IEnumerable<string>> GetActiveNamespaces();

    Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetRbacAssessmentReportDenormalizedDtos(
        string? namespaceName = null);

    Task<IEnumerable<RbacAssessmentReportDto>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null);
}
