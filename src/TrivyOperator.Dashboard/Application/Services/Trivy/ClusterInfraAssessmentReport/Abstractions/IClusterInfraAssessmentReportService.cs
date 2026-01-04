using TrivyOperator.Dashboard.Application.Models;

namespace TrivyOperator.Dashboard.Application.Services.Trivy.ClusterInfraAssessmentReport.Abstractions;

public interface IClusterInfraAssessmentReportService
{
    Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos();
    Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(Guid uid);
    Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetClusterInfraAssessmentReportDenormalizedDtos();
}