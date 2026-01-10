using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.RbacAssessmentReport;

public class RbacAssessmentReportNullService : IRbacAssessmentReportService
{
    public Task<IEnumerable<string>> GetActiveNamespaces() =>
        Task.FromResult<IEnumerable<string>>([]);

    public Task<IEnumerable<RbacAssessmentReportDenormalizedDto>> GetRbacAssessmentReportDenormalizedDtos(
        string? namespaceName = null) =>
        Task.FromResult<IEnumerable<RbacAssessmentReportDenormalizedDto>>([]);

    public Task<IEnumerable<RbacAssessmentReportDto>> GetRbacAssessmentReportDtos(
        string? namespaceName = null,
        IEnumerable<int>? excludedSeverities = null) =>
        Task.FromResult<IEnumerable<RbacAssessmentReportDto>>([]);
}
