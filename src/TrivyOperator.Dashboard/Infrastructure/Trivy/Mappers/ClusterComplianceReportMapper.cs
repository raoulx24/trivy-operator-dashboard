using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterComplianceReportMapper : ITrivyReportMapper<ClusterComplianceReportCr, ClusterComplianceReport>
{
    public ClusterComplianceReport MapToDomain(ClusterComplianceReportCr cr, ClusterComplianceReport? existing)
    {
        return cr.ToClusterComplianceReport(existing);
    }
}
