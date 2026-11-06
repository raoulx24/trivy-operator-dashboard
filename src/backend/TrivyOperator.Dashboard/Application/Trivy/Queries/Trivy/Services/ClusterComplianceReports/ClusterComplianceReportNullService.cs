using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterComplianceReports.Abstractions;
using TrivyOperator.Dashboard.Application.Shared.Queries;
using TrivyOperator.Dashboard.Application.Shared.Queries.Services;

namespace TrivyOperator.Dashboard.Application.Trivy.Queries.Trivy.Services.ClusterComplianceReports;

public class ClusterComplianceReportNullService : QueryNullService, IClusterComplianceReportService
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
