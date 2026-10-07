using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ConfigAuditReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain;

public class ClusterConfigAuditReportMapper :
    IResourceMapper<ClusterConfigAuditReportCr, ClusterConfigAuditReport>,
    IResourceKeyProvider<ClusterConfigAuditReportCr, Uid>
{
    public ClusterConfigAuditReport MapToDomain(ClusterConfigAuditReportCr cr, ClusterConfigAuditReport? existing)
    {
        return cr.ToSecurityAssessmentReport<ClusterConfigAuditReportCr, ClusterConfigAuditReport, Uid>(existing);
    }
    
    public Uid GetKey(ClusterConfigAuditReportCr kubernetesResource) => kubernetesResource.ToUidKey();
}
