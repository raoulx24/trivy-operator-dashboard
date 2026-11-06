using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;

public interface IClusterComplianceReportService : IQueryService
{
    Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos(CancellationToken ctx = default);
    Task<ClusterComplianceReportDto?> GetClusterComplianceReportDtoByUid(
        string uid,
        CancellationToken ctx = default);
    Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos(CancellationToken ctx = default);
}
