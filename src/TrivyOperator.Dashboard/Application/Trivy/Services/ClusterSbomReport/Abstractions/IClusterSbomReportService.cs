using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport.Abstractions;

public interface IClusterSbomReportService
{
    Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos();
    Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetClusterSbomReportDenormalizedDtos();
}
