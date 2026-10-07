using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;

public interface IClusterComplianceReportService
{
    Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos(CancellationToken ctx = default);
    Task<ClusterComplianceReportDto?> GetClusterComplianceReportDtoByUid(
        string uid,
        CancellationToken ctx = default);
    Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos(CancellationToken ctx = default);
}
