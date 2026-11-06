using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterSbomReports.Abstractions;

public interface IClusterSbomReportService : IQueryService
{
    Task<IEnumerable<SbomReportImageMinimalDto>> GetClusterSbomReportMinimalDtos(CancellationToken ctx = default);

    Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos(CancellationToken ctx = default);

    Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetClusterSbomReportDenormalizedDtos(CancellationToken ctx = default);
}
