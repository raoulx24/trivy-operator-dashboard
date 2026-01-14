using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport.Abstractions;

public interface IClusterInfraAssessmentReportService
{
    Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos();
    Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(Guid uid);
    Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetClusterInfraAssessmentReportDenormalizedDtos();
}
