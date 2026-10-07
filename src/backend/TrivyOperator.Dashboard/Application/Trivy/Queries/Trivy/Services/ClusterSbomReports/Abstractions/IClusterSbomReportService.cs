using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterSbomReports.Abstractions;

public interface IClusterSbomReportService
{
    Task<IEnumerable<SbomReportImageMinimalDto>> GetClusterSbomReportMinimalDtos(CancellationToken ctx = default);

    Task<IEnumerable<ClusterSbomReportDto>> GetClusterSbomReportDtos(CancellationToken ctx = default);

    Task<IEnumerable<ClusterSbomReportDenormalizedDto>> GetClusterSbomReportDenormalizedDtos(CancellationToken ctx = default);
}
