using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterSbomReports.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterSbomReports;

public class ClusterSbomReportNullService : QueryNullService, IClusterSbomReportService
{
    public Task<IEnumerable<SbomReportImageMinimalDto>> GetClusterSbomReportMinimalDtos(CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<SbomReportImageMinimalDto>>([]);

    public Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos(CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<ClusterSbomReportDto>>([]);

    public Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetClusterSbomReportDenormalizedDtos(CancellationToken ctx = default) 
        => Task.FromResult<IEnumerable<ClusterSbomReportDenormalizedDto>>([]);
}
