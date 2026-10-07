using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;
using TrivyOperator.Dashboard.Domain.Trivy.Entities;
using TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Abstract;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain.Extensions;
using TrivyOperator.Dashboard.Infrastructure.Trivy.Schema.InfraAssessmentReports;

namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.ToDomain;

public class ClusterInfraAssessmentReportMapper :
    IResourceMapper<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport>,
    IResourceKeyProvider<ClusterInfraAssessmentReportCr, Uid>
{
    public ClusterInfraAssessmentReport MapToDomain(ClusterInfraAssessmentReportCr cr, ClusterInfraAssessmentReport? existing)
    {
        return cr.ToSecurityAssessmentReport<ClusterInfraAssessmentReportCr, ClusterInfraAssessmentReport, Uid>(existing);
    }
    
    public Uid GetKey(ClusterInfraAssessmentReportCr kubernetesResource) => kubernetesResource.ToUidKey();
}
