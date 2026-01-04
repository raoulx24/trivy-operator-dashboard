using TrivyOperator.Dashboard.Application.Models;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.ClusterInfraAssessmentReport.Abstractions;

public interface IClusterInfraAssessmentReportService
{
    Task<IEnumerable<string>> GetActiveNamespaces();
    Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetClusterInfraAssessmentReportDenormalizedDtos(string? namespaceName = null);
    Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(Guid uid);
    Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos(string? namespaceName = null, IEnumerable<int>? excludedSeverities = null);
}