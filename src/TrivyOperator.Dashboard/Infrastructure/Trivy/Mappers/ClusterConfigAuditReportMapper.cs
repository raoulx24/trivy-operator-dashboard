using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterConfigAuditReportMapper :
    ITrivyReportMapper<ClusterConfigAuditReportCr, ClusterConfigAuditReport>,
    ITrivyReportKeyProvider<ClusterConfigAuditReportCr, Uid>
{
    public ClusterConfigAuditReport MapToDomain(ClusterConfigAuditReportCr cr, ClusterConfigAuditReport? existing)
    {
        return cr.ToSecurityAssessmentReport<ClusterConfigAuditReportCr, ClusterConfigAuditReport, Uid>(existing);
    }
    
    public Uid GetKey(ClusterConfigAuditReportCr cr) => cr.ToUidKey();
}
