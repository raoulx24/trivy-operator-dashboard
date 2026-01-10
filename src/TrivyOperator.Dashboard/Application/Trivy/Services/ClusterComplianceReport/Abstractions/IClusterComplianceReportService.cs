using TrivyOperator.Dashboard.Application.Trivy.Models;

namespace TrivyOperator.Dashboard.Application.Trivy.Services.ClusterComplianceReport.Abstractions;

public interface IClusterComplianceReportService
{
    Task<IEnumerable<ClusterComplianceReportDenormalizedDto>> GetClusterComplianceReportDenormalizedDtos();
    Task<IEnumerable<ClusterComplianceReportDto>> GetClusterComplianceReportDtos();
}
