using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterComplianceReportMapper :
    ITrivyReportMapper<ClusterComplianceReportCr, ClusterComplianceReport>,
    ITrivyReportKeyProvider<ClusterComplianceReportCr, Uid>
{
    public ClusterComplianceReport MapToDomain(ClusterComplianceReportCr cr, ClusterComplianceReport? existing)
    {
        return cr.ToClusterComplianceReport(existing);
    }

    public Uid GetKey(ClusterComplianceReportCr cr) => cr.ToUidKey();
}
