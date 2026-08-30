using TrivyOperator.Dashboard.Application.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;

namespace TrivyOperator.Dashboard.Application.Queries.Trivy.Services.ClusterComplianceReports;

public class ClusterComplianceReportNullService : IClusterComplianceReportService
{
    public Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos(CancellationToken ctx = default) 
        => Task.FromResult<IEnumerable<ClusterComplianceReportDto>>([]);

    public Task<ClusterComplianceReportDto?> GetClusterComplianceReportDtoByUid(
        string uid,
        CancellationToken ctx = default)
        => Task.FromResult<ClusterComplianceReportDto?>(null); 

    public Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos(CancellationToken ctx = default)
        => Task.FromResult<IEnumerable<ClusterComplianceReportDenormalizedDto>>([]);
}
