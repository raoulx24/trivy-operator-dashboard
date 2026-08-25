using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterSbomReports;

public class ClusterSbomReportNullService : IClusterSbomReportService
{
    public Task<IEnumerable<SbomReportImageMinimalDto>> GetSbomReportImageMinimalDtos(CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<SbomReportImageMinimalDto>>([]);

    public Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos(CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<ClusterSbomReportDto>>([]);

    public Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetClusterSbomReportDenormalizedDtos(CancellationToken ctx = default) 
        => Task.FromResult<IEnumerable<ClusterSbomReportDenormalizedDto>>([]);
}
