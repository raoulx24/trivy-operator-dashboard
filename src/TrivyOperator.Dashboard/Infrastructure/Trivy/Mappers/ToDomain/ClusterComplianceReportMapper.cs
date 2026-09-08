using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.ClusterComplianceReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain;

public class ClusterComplianceReportMapper :
    IResourceMapper<ClusterComplianceReportCr, ClusterComplianceReport>,
    IResourceKeyProvider<ClusterComplianceReportCr, Uid>
{
    public ClusterComplianceReport MapToDomain(ClusterComplianceReportCr cr, ClusterComplianceReport? existing)
    {
        return cr.ToClusterComplianceReport(existing);
    }

    public Uid GetKey(ClusterComplianceReportCr kubernetesResource) => kubernetesResource.ToUidKey();
}
