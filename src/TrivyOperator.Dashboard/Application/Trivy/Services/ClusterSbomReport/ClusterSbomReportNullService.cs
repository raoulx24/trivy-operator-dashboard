using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterSbomReport;

public class ClusterSbomReportNullService : IClusterSbomReportService
{
    public Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos()
        => Task.FromResult<IEnumerable<ClusterSbomReportDto>>([]);
    public Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetClusterSbomReportDenormalizedDtos() 
        => Task.FromResult<IEnumerable<ClusterSbomReportDenormalizedDto>>([]);
}
