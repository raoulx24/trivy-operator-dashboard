using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterInfraAssessmentReport;

public class ClusterInfraAssessmentReportNullService : IClusterInfraAssessmentReportService
{
    public Task<IEnumerable<ClusterInfraAssessmentReportDto>> GetClusterInfraAssessmentReportDtos() =>
        Task.FromResult<IEnumerable<ClusterInfraAssessmentReportDto>>([]);

    public Task<ClusterInfraAssessmentReportDto?> GetClusterInfraAssessmentReportDtoByUid(Guid uid) =>
        Task.FromResult<ClusterInfraAssessmentReportDto?>(null);

    public Task<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>> GetClusterInfraAssessmentReportDenormalizedDtos() =>
        Task.FromResult<IEnumerable<ClusterInfraAssessmentReportDenormalizedDto>>([]);
}
