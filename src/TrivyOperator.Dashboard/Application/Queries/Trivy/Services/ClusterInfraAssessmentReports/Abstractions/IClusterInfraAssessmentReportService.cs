using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterInfraAssessmentReports.Abstractions;

public interface IClusterInfraAssessmentReportService
{
    Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos(CancellationToken ctx = default);
    Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(string uid, CancellationToken ctx = default);
    Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetClusterInfraAssessmentReportDenormalizedDtos(CancellationToken ctx = default);
}
