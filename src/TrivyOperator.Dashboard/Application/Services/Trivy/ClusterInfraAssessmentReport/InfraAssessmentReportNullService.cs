using TrivyOperator.Dashboard.Application.Models;
using TrivyOperator.Dashboard.Application.Services.Trivy.ClusterInfraAssessmentReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.ClusterInfraAssessmentReport;

public class ClusterInfraAssessmentReportNullService : IClusterInfraAssessmentReportService
{
    public Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos(
        string? namespaceName = null, 
        IEnumerable<int>? excludedSeverities = null) => Task.FromResult<IEnumerable<ClusterInfraAssessmentReportDto>>([]);

    public Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(Guid uid) =>
        Task.FromResult<ClusterInfraAssessmentReportDto?>(null);

    public Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetClusterInfraAssessmentReportDenormalizedDtos(
        string? namespaceName = null) => Task.FromResult<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>([]);

    public Task<IEnumerable<string>> GetActiveNamespaces() => Task.FromResult<IEnumerable<string>>([]);

}
