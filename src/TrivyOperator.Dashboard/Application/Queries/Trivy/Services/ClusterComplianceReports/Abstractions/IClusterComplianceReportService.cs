using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;

public interface IClusterComplianceReportService
{
    Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos(CancellationToken ctx = default);
    Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos(CancellationToken ctx = default);
}
