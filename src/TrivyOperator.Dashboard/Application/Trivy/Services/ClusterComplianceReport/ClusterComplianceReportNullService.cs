using TrivyOperator.Dashboard.Application.Trivy.Models;
using TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport.Abstractions;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport;

public class ClusterComplianceReportNullService : IClusterComplianceReportService
{
    public Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos() =>
        Task.FromResult<IEnumerable<ClusterComplianceReportDto>>([]);

    public Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos() =>
        Task.FromResult<IEnumerable<ClusterComplianceReportDenormalizedDto>>([]);
}
