using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.K8s.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.SbomReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers;

public class ClusterSbomReportMapper :
    IResourceMapper<ClusterSbomReportCr, ClusterSbomReport>,
    IResourceKeyProvider<ClusterSbomReportCr, Uid>
{
    public ClusterSbomReport MapToDomain(ClusterSbomReportCr cr, ClusterSbomReport? existing)
    {
        return cr.ToClusterSbom(existing);
    }
    
    public Uid GetKey(ClusterSbomReportCr kubernetesResource) => kubernetesResource.ToUidKey();
}
